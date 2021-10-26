/* 
    To get table schema you need run this below in console application:

		SqlServerStorage sqlServerStorage = new SqlServerStorage("");
		var sqlScripts = sqlServerStorage.TableCreationScripts;

		sqlScripts.ForEach(sql => Console.WriteLine(sql));
		Console.ReadLine();

*/
CREATE TABLE MiniProfilers
(
    RowId                                integer not null identity constraint PK_MiniProfilers primary key clustered, -- Need a clustered primary key for SQL Azure
    Id                                   uniqueidentifier not null, -- don't cluster on a guid
    RootTimingId                         uniqueidentifier null,
    Name                                 nvarchar(200) null,
    Started                              datetime not null,
    DurationMilliseconds                 decimal(15,1) not null,
    [User]                               nvarchar(100) null,
    HasUserViewed                        bit not null,
    MachineName                          nvarchar(100) null,
    CustomLinksJson                      nvarchar(max),
    ClientTimingsRedirectCount           int null
);
-- displaying results selects everything based on the main MiniProfilers.Id column
CREATE UNIQUE NONCLUSTERED INDEX IX_MiniProfilers_Id ON MiniProfilers (Id);

-- speeds up a query that is called on every .Stop()
CREATE NONCLUSTERED INDEX IX_MiniProfilers_User_HasUserViewed_Includes ON MiniProfilers ([User], HasUserViewed) INCLUDE (Id, [Started]);

CREATE TABLE MiniProfilerTimings
(
    RowId                               integer not null identity constraint PK_MiniProfilerTimings primary key clustered,
    Id                                  uniqueidentifier not null,
    MiniProfilerId                      uniqueidentifier not null,
    ParentTimingId                      uniqueidentifier null,
    Name                                nvarchar(200) not null,
    DurationMilliseconds                decimal(15,3) not null,
    StartMilliseconds                   decimal(15,3) not null,
    IsRoot                              bit not null,
    Depth                               smallint not null,
    CustomTimingsJson                   nvarchar(max) null
);

CREATE UNIQUE NONCLUSTERED INDEX IX_MiniProfilerTimings_Id ON MiniProfilerTimings (Id);
CREATE NONCLUSTERED INDEX IX_MiniProfilerTimings_MiniProfilerId ON MiniProfilerTimings (MiniProfilerId);

CREATE TABLE MiniProfilerClientTimings
(
    RowId                               integer not null identity constraint PK_MiniProfilerClientTimings primary key clustered,
    Id                                  uniqueidentifier not null,
    MiniProfilerId                      uniqueidentifier not null,
    Name                                nvarchar(200) not null,
    Start                               decimal(9, 3) not null,
    Duration                            decimal(9, 3) not null
);

CREATE UNIQUE NONCLUSTERED INDEX IX_MiniProfilerClientTimings_Id on MiniProfilerClientTimings (Id);
CREATE NONCLUSTERED INDEX IX_MiniProfilerClientTimings_MiniProfilerId on MiniProfilerClientTimings (MiniProfilerId);