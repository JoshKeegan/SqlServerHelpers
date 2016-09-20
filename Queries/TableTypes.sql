/*
	SQL Server Generic Table Types
		Must be run on the database you're targeting before you use the generic table values
		e.g. before parameters.AddWithValue("@ids", IEnumerable<object> ids, ID_FIELD);
	Authors:
		Josh Keegan 20/09/2016
*/

CREATE TYPE dbo.TableType_Generic_BigInt AS TABLE
(
	v bigint NOT NULL
);

CREATE TYPE dbo.TableType_Generic_BigInt_Nullable AS TABLE
(
	v bigint NULL
);

CREATE TYPE dbo.TableType_Generic_VarChar_Nullable AS TABLE
(
	v varchar(max) NULL
);

CREATE TYPE dbo.TableType_Generic_VarChar AS TABLE
(
	v varchar(max) NOT NULL
);