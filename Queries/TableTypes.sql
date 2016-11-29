/*
	SQL Server Generic Table Types
		Must be run on the database you're targeting before you use the generic table values
		e.g. before parameters.AddWithValue("@ids", IEnumerable<object> ids, ID_FIELD);
	Authors:
		Josh Keegan 20/09/2016
*/

IF TYPE_ID('dbo.TableType_Generic_Int') IS NOT NULL
	DROP TYPE dbo.TableType_Generic_Int;
CREATE TYPE dbo.TableType_Generic_Int AS TABLE
(
	v int NOT NULL
);

IF TYPE_ID('dbo.TableType_Generic_Int_Nullable') IS NOT NULL
	DROP TYPE dbo.TableType_Generic_Int_Nullable;
CREATE TYPE dbo.TableType_Generic_Int_Nullable AS TABLE
(
	v int NULL
);

IF TYPE_ID('dbo.TableType_Generic_BigInt') IS NOT NULL
	DROP TYPE dbo.TableType_Generic_BigInt;
CREATE TYPE dbo.TableType_Generic_BigInt AS TABLE
(
	v bigint NOT NULL
);

IF TYPE_ID('dbo.TableType_Generic_BigInt_Nullable') IS NOT NULL
	DROP TYPE dbo.TableType_Generic_BigInt_Nullable;
CREATE TYPE dbo.TableType_Generic_BigInt_Nullable AS TABLE
(
	v bigint NULL
);

IF TYPE_ID('dbo.TableType_Generic_VarChar_Nullable') IS NOT NULL
	DROP TYPE dbo.TableType_Generic_VarChar_Nullable;
CREATE TYPE dbo.TableType_Generic_VarChar_Nullable AS TABLE
(
	v varchar(max) NULL
);

IF TYPE_ID('dbo.TableType_Generic_VarChar') IS NOT NULL
	DROP TYPE dbo.TableType_Generic_VarChar;
CREATE TYPE dbo.TableType_Generic_VarChar AS TABLE
(
	v varchar(max) NOT NULL
);