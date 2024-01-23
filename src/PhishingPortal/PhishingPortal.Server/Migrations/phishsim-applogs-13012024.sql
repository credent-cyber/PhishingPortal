CREATE TABLE `applogs` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(50) DEFAULT NULL,
  `Message` text,
  `ErrorDetail` longtext,
  `IsEmailed` tinyint DEFAULT NULL,
  `CreatedOn` datetime NOT NULL,
  `CreatedBy` varchar(100) DEFAULT NULL,
  `ModifiedOn` datetime DEFAULT NULL,
  `ModifiedBy` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
