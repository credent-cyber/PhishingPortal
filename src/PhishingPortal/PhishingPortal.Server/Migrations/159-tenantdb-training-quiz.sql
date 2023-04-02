CREATE TABLE `t-20220619003439`.`trainingquiz` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `TrainingId` INT NOT NULL,
  `Question` TEXT NULL,
  `AnswerType` VARCHAR(45) NULL,
  `OrderNumber` INT NULL,
  `Weightage` DECIMAL NULL,
  `IsActive` VARCHAR(45) NULL,
  PRIMARY KEY (`Id`));



CREATE TABLE `t-20220619003439`.`trainingquizanswer` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `QuestionId` INT NOT NULL,
  `AnswerText` TEXT NULL,
  `OrderNumber` INT NULL,
  `IsCorrect` INT NULL,
  PRIMARY KEY (`Id`));


ALTER TABLE `t-20220619003439`.`trainingquizanswer` 
ADD UNIQUE INDEX `QuestionId_UNIQUE` (`QuestionId` ASC) VISIBLE;



ALTER TABLE `t-20220619003439`.`trainingquizanswer` 
ADD CONSTRAINT `FK_trainingquizquestionanswers`
  FOREIGN KEY (`QuestionId`)
  REFERENCES `t-20220619003439`.`trainingquiz` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;


  

ALTER TABLE `t-20220619003439`.`trainingquiz` 
ADD INDEX `FK_trainingquizquestions_idx` (`TrainingId` ASC) VISIBLE;
;
ALTER TABLE `t-20220619003439`.`trainingquiz` 
ADD CONSTRAINT `FK_trainingquizquestions`
  FOREIGN KEY (`TrainingId`)
  REFERENCES `t-20220619003439`.`training` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;


  ALTER TABLE `t-20220619003439`.`traininglog` 
ADD COLUMN `UniqueID` VARCHAR(50) NOT NULL AFTER `SentOn`;