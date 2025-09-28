using System.IO;

return (dynamic networkManager, dynamic ENV) =>
{
	//networkManager.btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, "wifi.ico");
	networkManager.iconFile = "wifi.ico";
	Sambar.api.Print("NETWORK MANAFER: " + ENV.ASSETS_FOLDER);
};
