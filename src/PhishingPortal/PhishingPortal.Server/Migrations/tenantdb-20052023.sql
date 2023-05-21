ALTER TABLE `t-20220619003439`.`trainingquiz` 
RENAME TO  `t-20220619003439`.`trainingquizquestion` ;

CREATE TABLE `t-20220619003439`.`trainingquiz` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(250) NOT NULL,
  PRIMARY KEY (`Id`));


INSERT INTO `t-20220619003439`.`trainingquiz`
(`Name`)
VALUES( 'Quiz1');

ALTER TABLE `t-20220619003439`.`trainingquizquestion` 
ADD COLUMN `TrainingQuizId` INT NOT NULL;

UPDATE `t-20220619003439`.`trainingquizquestion` 
SET TrainingQuizId = 1
WHERE 1 = 1


ALTER TABLE `t-20220619003439`.`trainingquizquestion` 
ADD CONSTRAINT `TrainingQuiz_TrainingQuiz_Question_FK`
  FOREIGN KEY (`TrainingQuizId`)
  REFERENCES `t-20220619003439`.`trainingquiz` (`Id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;


  ALTER TABLE `t-20220619003439`.`training` 
ADD COLUMN `TrainingQuizId` INT NULL AFTER `TrainingScheduleId`;

