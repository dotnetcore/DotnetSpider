namespace DotnetSpider.Extension
{
	public class Settings
	{
		public static string MySqlDatabase = "CREATE DATABASE IF NOT EXISTS `dotnetspider` DEFAULT CHARACTER SET utf8;";
		public static string DropMySqlDatabase = "DROP DATABASE IF EXISTS `dotnetspider`";
		public static string MySqlSettingTable = "CREATE TABLE IF NOT EXISTS `dotnetspider`.`settings` (`id` int(11) NOT NULL AUTO_INCREMENT,`type` varchar(45) NOT NULL,`key` varchar(45) DEFAULT NULL,`value` text,PRIMARY KEY(`id`),UNIQUE KEY `UNIQUE` (`key`,`type`)) ENGINE=InnoDB AUTO_INCREMENT = 1 DEFAULT CHARSET = utf8;";
	}
}
