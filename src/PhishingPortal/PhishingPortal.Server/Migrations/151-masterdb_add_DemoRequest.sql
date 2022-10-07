use phishsim;
CREATE TABLE demorequestor (
  Id int NOT NULL auto_increment UNIQUE,
  FirstName varchar(45) NOT NULL,
  LastName varchar(45),
  Email varchar(45) NOT NULL,
  ContactNumber int,
  Messages varchar(255),
  IsNotified boolean DEFAULT false,
  CreatedBy varchar(45),
  CreatedOn datetime,
  ModifiedBy varchar(45),
  ModifiedOn datetime
);