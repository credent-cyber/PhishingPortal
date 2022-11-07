use phishsim;
CREATE TABLE demorequestor (
  Id int NOT NULL auto_increment UNIQUE,
  FullName varchar(45) NOT NULL,
  Email varchar(45) NOT NULL,
  ContactNumber int,
  Company varchar(45),
  Messages varchar(255),
  IsNotified boolean DEFAULT false,
  CreatedBy varchar(45),
  CreatedOn datetime,
  ModifiedBy varchar(45),
  ModifiedOn datetime
);