use `t-20220619003439`;

ALTER TABLE recipients
ADD COLUMN IsOnPremiseADUser TINYINT DEFAULT 0;

ALTER TABLE `t-20220619003439`.`recipients` 
ADD COLUMN `OnPremiseADUsername` TEXT NULL AFTER `IsOnPremiseADUser`;

ALTER TABLE `phishsim`.`aspnetusers` 
ADD COLUMN `ActiveDirectoryUsername` TEXT NULL AFTER `AccessFailedCount`,
ADD COLUMN `IsOnPremADUser` BIT NULL AFTER `ActiveDirectoryUsername`;
