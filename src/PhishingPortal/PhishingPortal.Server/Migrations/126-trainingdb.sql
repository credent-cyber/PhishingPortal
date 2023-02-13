

CREATE TABLE Training (
  Id int NOT NULL auto_increment UNIQUE,
  TrainingName varchar(45) NOT NULL,
  Content LONGTEXT NOT NULL,
  IsActive bool,
  CreatedBy datetime,
  CreatedOn datetime,
  ModifiedBy varchar(45),
  ModifiedOn datetime
);

CREATE TABLE TrainingLog (
  Id int NOT NULL auto_increment UNIQUE,
  TrainingID varchar(45) NOT NULL,
  UserID varchar(45) NOT NULL,
  PercentComplete decimal,
  Status varchar(45) NOT NULL,
  CreatedBy datetime,
  CreatedOn datetime,
  ModifiedBy varchar(45),
  ModifiedOn datetime
);