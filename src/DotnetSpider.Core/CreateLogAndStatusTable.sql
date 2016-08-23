CREATE DATABASE IF NOT EXISTS `dotnetspider`  DEFAULT CHARACTER SET utf8;
CREATE TABLE IF NOT EXISTS `dotnetspider`.`log` (
  `identity` varchar(100) DEFAULT NULL,
  `taskgroup` varchar(30) DEFAULT NULL,
  `userid` varchar(30) DEFAULT NULL,
  `logged` varchar(30) DEFAULT NULL,
  `level` varchar(10) DEFAULT NULL,
  `message` text,
  `callSite` text,
  `exception` text,
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`id`),
  KEY `index01` (`identity`)
) ENGINE=MyISAM AUTO_INCREMENT=1950900 DEFAULT CHARSET=utf8;
CREATE TABLE IF NOT EXISTS `dotnetspider`.`status` (
  `taskgroup` varchar(30) DEFAULT NULL,
  `userid` varchar(30) DEFAULT NULL,
  `identity` varchar(50) DEFAULT NULL,
  `status` varchar(20) DEFAULT NULL,
  `message` varchar(200) DEFAULT NULL,
  `logged` timestamp NULL DEFAULT NULL,
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uid` (`taskgroup`,`userid`,`identity`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;
