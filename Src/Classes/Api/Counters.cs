/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace sambar;

public partial class Api
{

	int cpuCores = 0;
	public void CountersInit()
	{
		cpuCores = GetCpuCount();
		//StartCpuMonitor();
		//StartNetworkMonitor();
		//StartMemoryMonitor();
	}

	Task? cpuTask, netTask, memTask;
	public bool StartCounters(string countersToStart, int clockTime)
	{
		bool success = false;
		var counters = countersToStart.Split(",");
		if (counters.Contains("cpu") && cpuTask == null)
		{
			cpuTask = StartCpuMonitor(clockTime);
			success = cpuTask?.Status == TaskStatus.Running;
		}
		if (counters.Contains("net") && netTask == null)
		{
			netTask = StartNetworkMonitor(clockTime);
			success = netTask?.Status == TaskStatus.Running;
		}
		if (counters.Contains("mem") && memTask == null)
		{
			memTask = StartMemoryMonitor(clockTime);
			success = memTask?.Status == TaskStatus.Running;
		}
		return success;
	}

	public int GetCpuCount()
	{
		SYSTEM_BASIC_INFORMATION sbi = new();
		Ntdll.NtQuerySystemInformation(
			SYSTEM_INFORMATION_CLASS.SystemBasicInformation,
			ref sbi,
			(uint)Marshal.SizeOf<SYSTEM_BASIC_INFORMATION>(),
			out uint returnLength
		);
		int coreCount = sbi.NumberOfProcessors;
		Logger.Log($"CORECOUNT: {coreCount}");
		return coreCount;
	}

	public delegate void CpuPerformanceEventHandler(long[] cpuStats);
	public event CpuPerformanceEventHandler CPU_PERFORMANCE_NOTIFIED = (stats) => { };
	/// <summary>
	/// Get CPU Usages
	/// https://www.codeproject.com/Articles/9113/Get-CPU-Usage-with-GetSystemTimes
	/// </summary>
	public Task? StartCpuMonitor(int clockTime)
	{
		if (cpuCores == 0) return null;

		CancellationTokenSource cts = new();
		long[] cpuStats = new long[cpuCores];
		return Task.Run(async () =>
		{
			long[] usr = new long[cpuCores];
			long[] kernel = new long[cpuCores];
			long[] idle = new long[cpuCores];

			long[] _usr = new long[cpuCores];
			long[] _kernel = new long[cpuCores];
			long[] _idle = new long[cpuCores];

			long[] _delta_usr = new long[cpuCores];
			long[] _delta_kernel = new long[cpuCores];
			long[] _delta_idle = new long[cpuCores];

			while (true)
			{
				int size = Marshal.SizeOf<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>();
				nint bufferPtr = Marshal.AllocHGlobal(size * cpuCores);
				Ntdll.NtQuerySystemInformation(
					SYSTEM_INFORMATION_CLASS.SystemProcessorPerformanceInformation,
					bufferPtr,
					(uint)(size * cpuCores),
					out uint returnLength
				);

				for (int i = 0; i < cpuCores; i++)
				{
					SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION proc =
						Marshal.PtrToStructure<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>(bufferPtr + size * i);

					_usr[i] = usr[i];
					_kernel[i] = kernel[i];
					_idle[i] = idle[i];

					usr[i] = proc.UserTime;
					kernel[i] = proc.KernelTime;
					idle[i] = proc.IdleTime;

					_delta_usr[i] = usr[i] - _usr[i];
					_delta_kernel[i] = kernel[i] - _kernel[i];
					_delta_idle[i] = idle[i] - _idle[i];

					long total = _delta_usr[i] + _delta_kernel[i];
					long active = total - _delta_idle[i];
					cpuStats[i] = active * 100 / total;
				}

				CPU_PERFORMANCE_NOTIFIED(cpuStats);
				long cpuTotalUsage = 0;
				cpuStats.ToList().ForEach(x => cpuTotalUsage += x);
				Marshal.FreeHGlobal(bufferPtr);
				//Logger.Log($"CPU TOTAL: {cpuTotalUsage / cpuCores}%");
				await Task.Delay(clockTime);
			}
		}, cts.Token);
	}


	public delegate void NetworkSpeedEventHandler(float[] speeds);
	public event NetworkSpeedEventHandler NETWORK_SPEED_NOTIFIED = (speeds) => { };
	/// <summary>
	/// Network monitor
	/// </summary>
	public Task? StartNetworkMonitor(int clockTime)
	{
		CancellationTokenSource cts = new();
		return Task.Run(async () =>
		{
			Logger.Log($"STARTING NETWORK MONITOR");
			var primaryInterface = Utils.GetPrimaryNetworkInterface();
			long downBytes = 0, _downBytes = 0, upBytes = 0, _upBytes = 0, _delta_downBytes = 0, _delta_upBytes = 0;
			int DELTA = clockTime; // milliseconds
			while (true)
			{
				_downBytes = downBytes;
				_upBytes = upBytes;

				downBytes = primaryInterface.GetIPv4Statistics().BytesReceived;
				upBytes = primaryInterface.GetIPv4Statistics().BytesSent;

				_delta_downBytes = downBytes - _downBytes;
				_delta_upBytes = upBytes - _upBytes;

				// speeds are in Kb/s
				float speedDown = ((float)_delta_downBytes * 8) / (DELTA / 1000) / 1024;
				float speedUp = ((float)_delta_upBytes * 8) / (DELTA / 1000) / 1024;
				NETWORK_SPEED_NOTIFIED([speedDown, speedUp]);
				//Logger.Log($"DOWN: {speedDown} Kb/s, UP: {speedUp} Kb/s");
				await Task.Delay(DELTA);
			}
		}, cts.Token);
	}

	public delegate void MemoryUsageEventHandler(float[] memoryUsage);
	public event MemoryUsageEventHandler MEMORY_USAGE_NOTIFIED = (memoryUsage) => { };
	/// <summary>
	/// Memory usage monirtor
	/// </summary>
	public Task? StartMemoryMonitor(int clockTime)
	{
		CancellationTokenSource cts = new();
		return Task.Run(async () =>
		{
			int infoSize = Marshal.SizeOf<_SYSTEM_MEMORY_USAGE_INFORMATION>();
			while (true)
			{
				nint infoPtr = Marshal.AllocHGlobal(infoSize);
				Ntdll.NtQuerySystemInformation(
					SYSTEM_INFORMATION_CLASS.SystemMemoryUsageInformation,
					infoPtr,
					(uint)infoSize,
					out uint returnLength
				);

				var info = Marshal.PtrToStructure<_SYSTEM_MEMORY_USAGE_INFORMATION>(infoPtr);


				float commited = (float)info.CommittedBytes / 1024 / 1024 / 1024; // in GB
				float available = (float)info.AvailableBytes / 1024 / 1024 / 1024;
				float totalPhysical = (float)info.TotalPhysicalBytes / 1024 / 1024 / 1024;
				//Logger.Log($"[ MEMORY ], commited: {commited} Gb, available: {available} Gb, totalPhysical: {totalPhysical} Gb ");
				Marshal.FreeHGlobal(infoPtr);
				MEMORY_USAGE_NOTIFIED([available, totalPhysical]);
				await Task.Delay(clockTime);
			}
		}, cts.Token);
	}
}



