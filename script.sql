/**** Create an access to the server machine ****/
USE [master]
drop database finplanweb
go
create database finplanweb
go
IF EXISTS(SELECT * FROM sys.syslogins
     WHERE name = N'db_access_user')
     DROP LOGIN db_access_user
GO

CREATE LOGIN db_access_user WITH PASSWORD = 'Depth81guard' ; 
GO
ALTER LOGIN db_access_user WITH DEFAULT_DATABASE = finplanweb
GO

USE finplanweb
GO
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'db_access_user')
DROP USER [db_access_user]
GO
CREATE USER db_access_user FROM LOGIN db_access_user
EXEC sp_addrolemember 'db_datareader', db_access_user
GO
EXEC sp_addrolemember 'db_datawriter', db_access_user
GO


alter table categories nocheck constraint all

/**** Categories Table ****/
USE [finplanweb]
GO

ALTER TABLE [dbo].[categories] DROP CONSTRAINT [DF__categorie__date___5535A963]
GO

/****** Object:  Table [dbo].[categories]    Script Date: 3/19/2014 3:35:22 PM ******/
DROP TABLE [dbo].[categories]
GO

/****** Object:  Table [dbo].[categories]    Script Date: 3/19/2014 3:35:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[categories](
	[categoriesID] [int] IDENTITY(1,1) NOT NULL,
	[categoriesName] [nvarchar](50) NOT NULL,
	[date_added] [datetime] NOT NULL,
	[last_modified] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[categoriesID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[categories] ADD  DEFAULT (getdate()) FOR [date_added]
GO


/****  Product to Categories Table *****/


USE [finplanweb]
GO

ALTER TABLE [dbo].[prodtocater] DROP CONSTRAINT [fk_PID]
GO

ALTER TABLE [dbo].[prodtocater] DROP CONSTRAINT [fk_CID]
GO

/****** Object:  Table [dbo].[prodtocater]    Script Date: 3/19/2014 3:25:49 PM ******/
DROP TABLE [dbo].[prodtocater]
GO

/****** Object:  Table [dbo].[prodtocater]    Script Date: 3/19/2014 3:25:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[prodtocater](
	[productID] [int] NOT NULL,
	[categoriesID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[productID] ASC,
	[categoriesID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[prodtocater]  WITH CHECK ADD  CONSTRAINT [fk_CID] FOREIGN KEY([categoriesID])
REFERENCES [dbo].[categories] ([categoriesID])
GO

ALTER TABLE [dbo].[prodtocater] CHECK CONSTRAINT [fk_CID]
GO

ALTER TABLE [dbo].[prodtocater]  WITH CHECK ADD  CONSTRAINT [fk_PID] FOREIGN KEY([productID])
REFERENCES [dbo].[products] ([productID])
GO

ALTER TABLE [dbo].[prodtocater] CHECK CONSTRAINT [fk_PID]
GO


/**** Products Table ****/
USE [finplanweb]
GO

ALTER TABLE [dbo].[products] DROP CONSTRAINT [DF__products__addedD__2E1BDC42]
GO

/****** Object:  Table [dbo].[products]    Script Date: 3/19/2014 3:38:28 PM ******/
DROP TABLE IF EXISTS [dbo].[products]
GO

/****** Object:  Table [dbo].[products]    Script Date: 3/19/2014 3:38:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE IF EXISTS [dbo].[products](
	[productID] [int] IDENTITY(1,1) NOT NULL,
	[productCode] [nvarchar](40) NOT NULL,
	[description] [nvarchar](300) NOT NULL,
	[addedDate] [datetime] NOT NULL,
	[modifiedDate] [datetime] NULL,
	[price] [money] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[productID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[productCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[products] ADD  DEFAULT (getdate()) FOR [addedDate]
GO


/**** Users Table ****/
USE [finplanweb]
GO

ALTER TABLE [dbo].[users] DROP CONSTRAINT [DF__users__RegDate__108B795B]
GO

/****** Object:  Table [dbo].[users]    Script Date: 3/19/2014 3:45:13 PM ******/
DROP TABLE [dbo].[users]
GO

/****** Object:  Table [dbo].[users]    Script Date: 3/19/2014 3:45:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](max) NOT NULL,
	[RegDate] [datetime] NULL,
	[Email] [nvarchar](50) NOT NULL,
	[isAdmin] [bit] NULL,
	[lastlogin] [datetime] NULL,
	[iplog] [varchar](15) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[users] ADD  DEFAULT (getdate()) FOR [RegDate]
GO


/**** Orders Table****/
USE [finplanweb]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [FK__orders__userID__34C8D9D1]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__dateModi__44FF419A]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__dateCrea__440B1D61]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__mcCurren__4316F928]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__mcGross__4222D4EF]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__paymentT__412EB0B6]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__paymentS__403A8C7D]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__paymentD__3F466844]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__qty__3E52440B]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__itemName__3D5E1FD2]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__txnType__3C69FB99]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__txnID__3B75D760]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__payerSta__3A81B327]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__payerBus__398D8EEE]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__payerEma__38996AB5]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__payerID__37A5467C]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__lastName__36B12243]
GO

ALTER TABLE [dbo].[orders] DROP CONSTRAINT [DF__orders__firstNam__35BCFE0A]
GO

/****** Object:  Table [dbo].[orders]    Script Date: 3/19/2014 4:03:49 PM ******/
DROP TABLE [dbo].[orders]
GO

/****** Object:  Table [dbo].[orders]    Script Date: 3/19/2014 4:03:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[orders](
	[orderID] [int] IDENTITY(1,1) NOT NULL,
	[userID] [int] NULL,
	[firstName] [nvarchar](50) NULL,
	[lastName] [nvarchar](50) NULL,
	[payerID] [nvarchar](13) NULL,
	[payerEmail] [nvarchar](50) NULL,
	[payerBusiness] [nvarchar](50) NULL,
	[payerStatus] [nvarchar](50) NULL,
	[txnID] [nvarchar](50) NULL,
	[txnType] [nvarchar](20) NULL,
	[itemName] [ntext] NULL,
	[qty] [ntext] NULL,
	[paymentDate] [datetime] NULL,
	[paymentStatus] [nvarchar](50) NULL,
	[paymentType] [nvarchar](40) NULL,
	[mcGross] [money] NULL,
	[mcCurrency] [nvarchar](40) NULL,
	[dateCreated] [datetime] NULL,
	[dateModified] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[orderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [firstName]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [lastName]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [payerID]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [payerEmail]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [payerBusiness]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [payerStatus]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT ('') FOR [txnID]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [txnType]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [itemName]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [qty]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [paymentDate]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [paymentStatus]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [paymentType]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [mcGross]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [mcCurrency]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [dateCreated]
GO

ALTER TABLE [dbo].[orders] ADD  DEFAULT (NULL) FOR [dateModified]
GO

ALTER TABLE [dbo].[orders]  WITH CHECK ADD FOREIGN KEY([userID])
REFERENCES [dbo].[users] ([Id])
GO





