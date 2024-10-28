CREATE TABLE UserProfilePicUpld (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Email VARCHAR(255) NOT NULL,
    ProfileImage LONGBLOB,
    BackgroundImage LONGBLOB
    );