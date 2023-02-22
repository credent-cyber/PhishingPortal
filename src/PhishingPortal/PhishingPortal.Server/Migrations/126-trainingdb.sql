

CREATE TABLE Training (
  Id int NOT NULL auto_increment UNIQUE,
  TrainingName varchar(45) NOT NULL,
  TrainingCategory varchar(45) NOT NULL,
  Content LONGTEXT NOT NULL,
  IsActive bool,
  State int,
  CreatedBy datetime,
  CreatedOn datetime,
  ModifiedBy varchar(45),
  ModifiedOn datetime,
  TrainingScheduleId int,
  FOREIGN KEY (TrainingScheduleId) REFERENCES trainingschedule(Id)
  );

CREATE TABLE TrainingLog (
  Id int NOT NULL auto_increment UNIQUE,
  TrainingID int NOT NULL,
  ReicipientID varchar(45) NOT NULL,
  TrainingType varchar(45),
  PercentCompleted decimal,
  Status varchar(45) NOT NULL,
  SecurityStamp LONGTEXT,
  Url LONGTEXT,
  CreatedBy datetime,
  CreatedOn datetime,
  ModifiedBy varchar(45),
  ModifiedOn datetime,
  SentOn datetime
);

CREATE TABLE TrainingSchedule (
  Id int NOT NULL auto_increment primary key,
ScheduleType int,
WillRepeat tinyint(1),
ScheduleInfo LONGTEXT 
);

CREATE TABLE TrainingRecipient (
Id int NOT NULL auto_increment primary key,
TrainingId int,
 FOREIGN KEY (TrainingId) REFERENCES Training(Id),
RecipientId int,
FOREIGN KEY (RecipientId) REFERENCES recipients(Id),
RecipientGroupId int,
FOREIGN KEY (RecipientGroupId) REFERENCES recipientgroups(Id)
)