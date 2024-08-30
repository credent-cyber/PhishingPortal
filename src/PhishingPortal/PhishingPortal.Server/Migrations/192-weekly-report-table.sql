

-- for all tenant db with this feature.

CREATE TABLE WeeklyReport (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportContent TEXT,
    CreatedOn DATETIME,
    IsReportSent BOOLEAN
);