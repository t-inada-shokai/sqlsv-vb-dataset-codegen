CREATE TYPE [TYPE_D_SAMPLE_DATA] AS TABLE (
	  [uid] BIGINT NULL
	, [original_str] NVARCHAR(20) COLLATE Japanese_CI_AS NULL
	, [castAsBigInt] BIGINT NULL
	, [mul_2] BIGINT NULL
	, [mul_2_AsStr] NVARCHAR(20) COLLATE Japanese_CI_AS NULL
	, [updatedatetime] DATETIMEOFFSET(1) NULL
)

