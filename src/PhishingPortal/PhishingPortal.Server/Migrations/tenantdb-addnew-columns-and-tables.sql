-- for sql lite, the other databases will have different column types

Alter Table Recipients
Add IsADUser INTEGER DEFAULT 0;

Alter Table RecipientGroups
Add Uid TEXT DEFAULT '';

Alter Table RecipientGroups
Add IsActiveDirectoryGroup INTEGER DEFAULT 0;

Alter Table RecipientGroups
Add LastImported TEXT DEFAULT '';

CREATE TABLE "Settings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Settings" PRIMARY KEY AUTOINCREMENT,
    "Key" TEXT NULL,
    "Value" TEXT NULL,
    "CreatedBy" TEXT NULL,
    "CreatedOn" TEXT NOT NULL,
    "ModifiedBy" TEXT NULL,
    "ModifiedOn" TEXT NOT NULL
);

SELECT * FROM Recipients;


--- queries for my sql

Alter Table Recipients
Add IsADUser int DEFAULT 0;

Alter Table RecipientGroups
Add Uid CHARACTER DEFAULT '';

Alter Table RecipientGroups
Add IsActiveDirectoryGroup INTEGER DEFAULT 0;

Alter Table RecipientGroups
Add LastImported datetime(6) NULL;

CREATE TABLE `Settings` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedOn` datetime(6) NOT NULL,
  `ModifiedBy` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ModifiedOn` datetime(6) NULL,
   PRIMARY KEY (`Id`),
   unique(`Key`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


INSERT INTO `settings` (`Key`,`Value`,`CreatedBy`,`CreatedOn`)
VALUES('az_client_id','e1fea56c-ef5c-4e10-9fa6-ab28d077e34c', 'tenantadmin', current_date());

INSERT INTO `settings` (`Key`,`Value`,`CreatedBy`,`CreatedOn`)
VALUES('az_client_secret','~5A8Q~iYjzn188PLNj3kzdm8QVSPkwzS1owqBb.U', 'tenantadmin', current_date());

INSERT INTO `settings` (`Key`,`Value`,`CreatedBy`,`CreatedOn`)
VALUES('az_tenant_id','cf92019c-152d-42f6-bbcc-0cf96e6b0108', 'tenantadmin', current_date());

