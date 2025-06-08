using System.Resources;
using System.Runtime.CompilerServices;
using FxResources.System.Data.SqlClient;

namespace System;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(FxResources.System.Data.SqlClient.SR)));

	internal static string ADP_CollectionIndexInt32 => GetResourceString("ADP_CollectionIndexInt32");

	internal static string ADP_CollectionIndexString => GetResourceString("ADP_CollectionIndexString");

	internal static string ADP_CollectionInvalidType => GetResourceString("ADP_CollectionInvalidType");

	internal static string ADP_CollectionIsNotParent => GetResourceString("ADP_CollectionIsNotParent");

	internal static string ADP_CollectionNullValue => GetResourceString("ADP_CollectionNullValue");

	internal static string ADP_CollectionRemoveInvalidObject => GetResourceString("ADP_CollectionRemoveInvalidObject");

	internal static string ADP_ConnectionAlreadyOpen => GetResourceString("ADP_ConnectionAlreadyOpen");

	internal static string ADP_ConnectionStateMsg_Closed => GetResourceString("ADP_ConnectionStateMsg_Closed");

	internal static string ADP_ConnectionStateMsg_Connecting => GetResourceString("ADP_ConnectionStateMsg_Connecting");

	internal static string ADP_ConnectionStateMsg_Open => GetResourceString("ADP_ConnectionStateMsg_Open");

	internal static string ADP_ConnectionStateMsg_OpenExecuting => GetResourceString("ADP_ConnectionStateMsg_OpenExecuting");

	internal static string ADP_ConnectionStateMsg_OpenFetching => GetResourceString("ADP_ConnectionStateMsg_OpenFetching");

	internal static string ADP_ConnectionStateMsg => GetResourceString("ADP_ConnectionStateMsg");

	internal static string ADP_ConnectionStringSyntax => GetResourceString("ADP_ConnectionStringSyntax");

	internal static string ADP_DataReaderClosed => GetResourceString("ADP_DataReaderClosed");

	internal static string ADP_InternalConnectionError => GetResourceString("ADP_InternalConnectionError");

	internal static string ADP_InvalidEnumerationValue => GetResourceString("ADP_InvalidEnumerationValue");

	internal static string ADP_NotSupportedEnumerationValue => GetResourceString("ADP_NotSupportedEnumerationValue");

	internal static string ADP_InvalidOffsetValue => GetResourceString("ADP_InvalidOffsetValue");

	internal static string ADP_TransactionPresent => GetResourceString("ADP_TransactionPresent");

	internal static string ADP_LocalTransactionPresent => GetResourceString("ADP_LocalTransactionPresent");

	internal static string ADP_NoConnectionString => GetResourceString("ADP_NoConnectionString");

	internal static string ADP_OpenConnectionPropertySet => GetResourceString("ADP_OpenConnectionPropertySet");

	internal static string ADP_PendingAsyncOperation => GetResourceString("ADP_PendingAsyncOperation");

	internal static string ADP_PooledOpenTimeout => GetResourceString("ADP_PooledOpenTimeout");

	internal static string ADP_NonPooledOpenTimeout => GetResourceString("ADP_NonPooledOpenTimeout");

	internal static string ADP_SingleValuedProperty => GetResourceString("ADP_SingleValuedProperty");

	internal static string ADP_DoubleValuedProperty => GetResourceString("ADP_DoubleValuedProperty");

	internal static string ADP_InvalidPrefixSuffix => GetResourceString("ADP_InvalidPrefixSuffix");

	internal static string Arg_ArrayPlusOffTooSmall => GetResourceString("Arg_ArrayPlusOffTooSmall");

	internal static string Arg_RankMultiDimNotSupported => GetResourceString("Arg_RankMultiDimNotSupported");

	internal static string Arg_RemoveArgNotFound => GetResourceString("Arg_RemoveArgNotFound");

	internal static string ArgumentOutOfRange_NeedNonNegNum => GetResourceString("ArgumentOutOfRange_NeedNonNegNum");

	internal static string Data_InvalidOffsetLength => GetResourceString("Data_InvalidOffsetLength");

	internal static string SqlConvert_ConvertFailed => GetResourceString("SqlConvert_ConvertFailed");

	internal static string SQL_WrongType => GetResourceString("SQL_WrongType");

	internal static string ADP_DeriveParametersNotSupported => GetResourceString("ADP_DeriveParametersNotSupported");

	internal static string ADP_NoStoredProcedureExists => GetResourceString("ADP_NoStoredProcedureExists");

	internal static string ADP_InvalidConnectionOptionValue => GetResourceString("ADP_InvalidConnectionOptionValue");

	internal static string ADP_MissingConnectionOptionValue => GetResourceString("ADP_MissingConnectionOptionValue");

	internal static string ADP_InvalidConnectionOptionValueLength => GetResourceString("ADP_InvalidConnectionOptionValueLength");

	internal static string ADP_KeywordNotSupported => GetResourceString("ADP_KeywordNotSupported");

	internal static string ADP_InternalProviderError => GetResourceString("ADP_InternalProviderError");

	internal static string ADP_InvalidMultipartName => GetResourceString("ADP_InvalidMultipartName");

	internal static string ADP_InvalidMultipartNameQuoteUsage => GetResourceString("ADP_InvalidMultipartNameQuoteUsage");

	internal static string ADP_InvalidMultipartNameToManyParts => GetResourceString("ADP_InvalidMultipartNameToManyParts");

	internal static string SQL_SqlCommandCommandText => GetResourceString("SQL_SqlCommandCommandText");

	internal static string SQL_BatchedUpdatesNotAvailableOnContextConnection => GetResourceString("SQL_BatchedUpdatesNotAvailableOnContextConnection");

	internal static string SQL_BulkCopyDestinationTableName => GetResourceString("SQL_BulkCopyDestinationTableName");

	internal static string SQL_TDSParserTableName => GetResourceString("SQL_TDSParserTableName");

	internal static string SQL_TypeName => GetResourceString("SQL_TypeName");

	internal static string SQLMSF_FailoverPartnerNotSupported => GetResourceString("SQLMSF_FailoverPartnerNotSupported");

	internal static string SQL_NotSupportedEnumerationValue => GetResourceString("SQL_NotSupportedEnumerationValue");

	internal static string ADP_CommandTextRequired => GetResourceString("ADP_CommandTextRequired");

	internal static string ADP_ConnectionRequired => GetResourceString("ADP_ConnectionRequired");

	internal static string ADP_OpenConnectionRequired => GetResourceString("ADP_OpenConnectionRequired");

	internal static string ADP_TransactionConnectionMismatch => GetResourceString("ADP_TransactionConnectionMismatch");

	internal static string ADP_TransactionRequired => GetResourceString("ADP_TransactionRequired");

	internal static string ADP_OpenReaderExists => GetResourceString("ADP_OpenReaderExists");

	internal static string ADP_CalledTwice => GetResourceString("ADP_CalledTwice");

	internal static string ADP_InvalidCommandTimeout => GetResourceString("ADP_InvalidCommandTimeout");

	internal static string ADP_UninitializedParameterSize => GetResourceString("ADP_UninitializedParameterSize");

	internal static string ADP_PrepareParameterType => GetResourceString("ADP_PrepareParameterType");

	internal static string ADP_PrepareParameterSize => GetResourceString("ADP_PrepareParameterSize");

	internal static string ADP_PrepareParameterScale => GetResourceString("ADP_PrepareParameterScale");

	internal static string ADP_MismatchedAsyncResult => GetResourceString("ADP_MismatchedAsyncResult");

	internal static string ADP_ClosedConnectionError => GetResourceString("ADP_ClosedConnectionError");

	internal static string ADP_ConnectionIsDisabled => GetResourceString("ADP_ConnectionIsDisabled");

	internal static string ADP_EmptyDatabaseName => GetResourceString("ADP_EmptyDatabaseName");

	internal static string ADP_InvalidSourceBufferIndex => GetResourceString("ADP_InvalidSourceBufferIndex");

	internal static string ADP_InvalidDestinationBufferIndex => GetResourceString("ADP_InvalidDestinationBufferIndex");

	internal static string ADP_StreamClosed => GetResourceString("ADP_StreamClosed");

	internal static string ADP_InvalidSeekOrigin => GetResourceString("ADP_InvalidSeekOrigin");

	internal static string ADP_NonSequentialColumnAccess => GetResourceString("ADP_NonSequentialColumnAccess");

	internal static string ADP_InvalidDataType => GetResourceString("ADP_InvalidDataType");

	internal static string ADP_UnknownDataType => GetResourceString("ADP_UnknownDataType");

	internal static string ADP_UnknownDataTypeCode => GetResourceString("ADP_UnknownDataTypeCode");

	internal static string ADP_DbTypeNotSupported => GetResourceString("ADP_DbTypeNotSupported");

	internal static string ADP_VersionDoesNotSupportDataType => GetResourceString("ADP_VersionDoesNotSupportDataType");

	internal static string ADP_ParameterValueOutOfRange => GetResourceString("ADP_ParameterValueOutOfRange");

	internal static string ADP_BadParameterName => GetResourceString("ADP_BadParameterName");

	internal static string ADP_InvalidSizeValue => GetResourceString("ADP_InvalidSizeValue");

	internal static string ADP_NegativeParameter => GetResourceString("ADP_NegativeParameter");

	internal static string ADP_InvalidMetaDataValue => GetResourceString("ADP_InvalidMetaDataValue");

	internal static string ADP_ParameterConversionFailed => GetResourceString("ADP_ParameterConversionFailed");

	internal static string ADP_ParallelTransactionsNotSupported => GetResourceString("ADP_ParallelTransactionsNotSupported");

	internal static string ADP_TransactionZombied => GetResourceString("ADP_TransactionZombied");

	internal static string ADP_InvalidDataLength2 => GetResourceString("ADP_InvalidDataLength2");

	internal static string ADP_NonSeqByteAccess => GetResourceString("ADP_NonSeqByteAccess");

	internal static string ADP_InvalidMinMaxPoolSizeValues => GetResourceString("ADP_InvalidMinMaxPoolSizeValues");

	internal static string SQL_InvalidPacketSizeValue => GetResourceString("SQL_InvalidPacketSizeValue");

	internal static string SQL_NullEmptyTransactionName => GetResourceString("SQL_NullEmptyTransactionName");

	internal static string SQL_UserInstanceFailoverNotCompatible => GetResourceString("SQL_UserInstanceFailoverNotCompatible");

	internal static string SQL_EncryptionNotSupportedByClient => GetResourceString("SQL_EncryptionNotSupportedByClient");

	internal static string SQL_EncryptionNotSupportedByServer => GetResourceString("SQL_EncryptionNotSupportedByServer");

	internal static string SQL_InvalidSQLServerVersionUnknown => GetResourceString("SQL_InvalidSQLServerVersionUnknown");

	internal static string SQL_CannotCreateNormalizer => GetResourceString("SQL_CannotCreateNormalizer");

	internal static string SQL_CannotModifyPropertyAsyncOperationInProgress => GetResourceString("SQL_CannotModifyPropertyAsyncOperationInProgress");

	internal static string SQL_InstanceFailure => GetResourceString("SQL_InstanceFailure");

	internal static string SQL_InvalidPartnerConfiguration => GetResourceString("SQL_InvalidPartnerConfiguration");

	internal static string SQL_MarsUnsupportedOnConnection => GetResourceString("SQL_MarsUnsupportedOnConnection");

	internal static string SQL_NonLocalSSEInstance => GetResourceString("SQL_NonLocalSSEInstance");

	internal static string SQL_PendingBeginXXXExists => GetResourceString("SQL_PendingBeginXXXExists");

	internal static string SQL_NonXmlResult => GetResourceString("SQL_NonXmlResult");

	internal static string SQL_InvalidParameterTypeNameFormat => GetResourceString("SQL_InvalidParameterTypeNameFormat");

	internal static string SQL_InvalidParameterNameLength => GetResourceString("SQL_InvalidParameterNameLength");

	internal static string SQL_PrecisionValueOutOfRange => GetResourceString("SQL_PrecisionValueOutOfRange");

	internal static string SQL_ScaleValueOutOfRange => GetResourceString("SQL_ScaleValueOutOfRange");

	internal static string SQL_TimeScaleValueOutOfRange => GetResourceString("SQL_TimeScaleValueOutOfRange");

	internal static string SQL_ParameterInvalidVariant => GetResourceString("SQL_ParameterInvalidVariant");

	internal static string SQL_ParameterTypeNameRequired => GetResourceString("SQL_ParameterTypeNameRequired");

	internal static string SQL_InvalidInternalPacketSize => GetResourceString("SQL_InvalidInternalPacketSize");

	internal static string SQL_InvalidTDSVersion => GetResourceString("SQL_InvalidTDSVersion");

	internal static string SQL_InvalidTDSPacketSize => GetResourceString("SQL_InvalidTDSPacketSize");

	internal static string SQL_ParsingError => GetResourceString("SQL_ParsingError");

	internal static string SQL_ConnectionLockedForBcpEvent => GetResourceString("SQL_ConnectionLockedForBcpEvent");

	internal static string SQL_SNIPacketAllocationFailure => GetResourceString("SQL_SNIPacketAllocationFailure");

	internal static string SQL_SmallDateTimeOverflow => GetResourceString("SQL_SmallDateTimeOverflow");

	internal static string SQL_TimeOverflow => GetResourceString("SQL_TimeOverflow");

	internal static string SQL_MoneyOverflow => GetResourceString("SQL_MoneyOverflow");

	internal static string SQL_CultureIdError => GetResourceString("SQL_CultureIdError");

	internal static string SQL_OperationCancelled => GetResourceString("SQL_OperationCancelled");

	internal static string SQL_SevereError => GetResourceString("SQL_SevereError");

	internal static string SQL_SSPIGenerateError => GetResourceString("SQL_SSPIGenerateError");

	internal static string SQL_KerberosTicketMissingError => GetResourceString("SQL_KerberosTicketMissingError");

	internal static string SQL_SqlServerBrowserNotAccessible => GetResourceString("SQL_SqlServerBrowserNotAccessible");

	internal static string SQL_InvalidSSPIPacketSize => GetResourceString("SQL_InvalidSSPIPacketSize");

	internal static string SQL_SSPIInitializeError => GetResourceString("SQL_SSPIInitializeError");

	internal static string SQL_Timeout => GetResourceString("SQL_Timeout");

	internal static string SQL_Timeout_PreLogin_Begin => GetResourceString("SQL_Timeout_PreLogin_Begin");

	internal static string SQL_Timeout_PreLogin_InitializeConnection => GetResourceString("SQL_Timeout_PreLogin_InitializeConnection");

	internal static string SQL_Timeout_PreLogin_SendHandshake => GetResourceString("SQL_Timeout_PreLogin_SendHandshake");

	internal static string SQL_Timeout_PreLogin_ConsumeHandshake => GetResourceString("SQL_Timeout_PreLogin_ConsumeHandshake");

	internal static string SQL_Timeout_Login_Begin => GetResourceString("SQL_Timeout_Login_Begin");

	internal static string SQL_Timeout_Login_ProcessConnectionAuth => GetResourceString("SQL_Timeout_Login_ProcessConnectionAuth");

	internal static string SQL_Timeout_PostLogin => GetResourceString("SQL_Timeout_PostLogin");

	internal static string SQL_Timeout_FailoverInfo => GetResourceString("SQL_Timeout_FailoverInfo");

	internal static string SQL_Timeout_RoutingDestinationInfo => GetResourceString("SQL_Timeout_RoutingDestinationInfo");

	internal static string SQL_Duration_PreLogin_Begin => GetResourceString("SQL_Duration_PreLogin_Begin");

	internal static string SQL_Duration_PreLoginHandshake => GetResourceString("SQL_Duration_PreLoginHandshake");

	internal static string SQL_Duration_Login_Begin => GetResourceString("SQL_Duration_Login_Begin");

	internal static string SQL_Duration_Login_ProcessConnectionAuth => GetResourceString("SQL_Duration_Login_ProcessConnectionAuth");

	internal static string SQL_Duration_PostLogin => GetResourceString("SQL_Duration_PostLogin");

	internal static string SQL_UserInstanceFailure => GetResourceString("SQL_UserInstanceFailure");

	internal static string SQL_InvalidRead => GetResourceString("SQL_InvalidRead");

	internal static string SQL_NonBlobColumn => GetResourceString("SQL_NonBlobColumn");

	internal static string SQL_NonCharColumn => GetResourceString("SQL_NonCharColumn");

	internal static string SQL_StreamNotSupportOnColumnType => GetResourceString("SQL_StreamNotSupportOnColumnType");

	internal static string SQL_TextReaderNotSupportOnColumnType => GetResourceString("SQL_TextReaderNotSupportOnColumnType");

	internal static string SQL_XmlReaderNotSupportOnColumnType => GetResourceString("SQL_XmlReaderNotSupportOnColumnType");

	internal static string SqlDelegatedTransaction_PromotionFailed => GetResourceString("SqlDelegatedTransaction_PromotionFailed");

	internal static string SQL_InvalidBufferSizeOrIndex => GetResourceString("SQL_InvalidBufferSizeOrIndex");

	internal static string SQL_InvalidDataLength => GetResourceString("SQL_InvalidDataLength");

	internal static string SQL_BulkLoadMappingInaccessible => GetResourceString("SQL_BulkLoadMappingInaccessible");

	internal static string SQL_BulkLoadMappingsNamesOrOrdinalsOnly => GetResourceString("SQL_BulkLoadMappingsNamesOrOrdinalsOnly");

	internal static string SQL_BulkLoadCannotConvertValue => GetResourceString("SQL_BulkLoadCannotConvertValue");

	internal static string SQL_BulkLoadNonMatchingColumnMapping => GetResourceString("SQL_BulkLoadNonMatchingColumnMapping");

	internal static string SQL_BulkLoadNonMatchingColumnName => GetResourceString("SQL_BulkLoadNonMatchingColumnName");

	internal static string SQL_BulkLoadStringTooLong => GetResourceString("SQL_BulkLoadStringTooLong");

	internal static string SQL_BulkLoadInvalidTimeout => GetResourceString("SQL_BulkLoadInvalidTimeout");

	internal static string SQL_BulkLoadInvalidVariantValue => GetResourceString("SQL_BulkLoadInvalidVariantValue");

	internal static string SQL_BulkLoadExistingTransaction => GetResourceString("SQL_BulkLoadExistingTransaction");

	internal static string SQL_BulkLoadNoCollation => GetResourceString("SQL_BulkLoadNoCollation");

	internal static string SQL_BulkLoadConflictingTransactionOption => GetResourceString("SQL_BulkLoadConflictingTransactionOption");

	internal static string SQL_BulkLoadInvalidOperationInsideEvent => GetResourceString("SQL_BulkLoadInvalidOperationInsideEvent");

	internal static string SQL_BulkLoadMissingDestinationTable => GetResourceString("SQL_BulkLoadMissingDestinationTable");

	internal static string SQL_BulkLoadInvalidDestinationTable => GetResourceString("SQL_BulkLoadInvalidDestinationTable");

	internal static string SQL_BulkLoadNotAllowDBNull => GetResourceString("SQL_BulkLoadNotAllowDBNull");

	internal static string Sql_BulkLoadLcidMismatch => GetResourceString("Sql_BulkLoadLcidMismatch");

	internal static string SQL_BulkLoadPendingOperation => GetResourceString("SQL_BulkLoadPendingOperation");

	internal static string SQL_CannotGetDTCAddress => GetResourceString("SQL_CannotGetDTCAddress");

	internal static string SQL_ConnectionDoomed => GetResourceString("SQL_ConnectionDoomed");

	internal static string SQL_OpenResultCountExceeded => GetResourceString("SQL_OpenResultCountExceeded");

	internal static string SQL_StreamWriteNotSupported => GetResourceString("SQL_StreamWriteNotSupported");

	internal static string SQL_StreamReadNotSupported => GetResourceString("SQL_StreamReadNotSupported");

	internal static string SQL_StreamSeekNotSupported => GetResourceString("SQL_StreamSeekNotSupported");

	internal static string SQL_ExClientConnectionId => GetResourceString("SQL_ExClientConnectionId");

	internal static string SQL_ExErrorNumberStateClass => GetResourceString("SQL_ExErrorNumberStateClass");

	internal static string SQL_ExOriginalClientConnectionId => GetResourceString("SQL_ExOriginalClientConnectionId");

	internal static string SQL_ExRoutingDestination => GetResourceString("SQL_ExRoutingDestination");

	internal static string SQL_UnsupportedSysTxVersion => GetResourceString("SQL_UnsupportedSysTxVersion");

	internal static string SqlMisc_NullString => GetResourceString("SqlMisc_NullString");

	internal static string SqlMisc_MessageString => GetResourceString("SqlMisc_MessageString");

	internal static string SqlMisc_ArithOverflowMessage => GetResourceString("SqlMisc_ArithOverflowMessage");

	internal static string SqlMisc_DivideByZeroMessage => GetResourceString("SqlMisc_DivideByZeroMessage");

	internal static string SqlMisc_NullValueMessage => GetResourceString("SqlMisc_NullValueMessage");

	internal static string SqlMisc_TruncationMessage => GetResourceString("SqlMisc_TruncationMessage");

	internal static string SqlMisc_DateTimeOverflowMessage => GetResourceString("SqlMisc_DateTimeOverflowMessage");

	internal static string SqlMisc_ConcatDiffCollationMessage => GetResourceString("SqlMisc_ConcatDiffCollationMessage");

	internal static string SqlMisc_CompareDiffCollationMessage => GetResourceString("SqlMisc_CompareDiffCollationMessage");

	internal static string SqlMisc_InvalidFlagMessage => GetResourceString("SqlMisc_InvalidFlagMessage");

	internal static string SqlMisc_NumeToDecOverflowMessage => GetResourceString("SqlMisc_NumeToDecOverflowMessage");

	internal static string SqlMisc_ConversionOverflowMessage => GetResourceString("SqlMisc_ConversionOverflowMessage");

	internal static string SqlMisc_InvalidDateTimeMessage => GetResourceString("SqlMisc_InvalidDateTimeMessage");

	internal static string SqlMisc_TimeZoneSpecifiedMessage => GetResourceString("SqlMisc_TimeZoneSpecifiedMessage");

	internal static string SqlMisc_InvalidArraySizeMessage => GetResourceString("SqlMisc_InvalidArraySizeMessage");

	internal static string SqlMisc_InvalidPrecScaleMessage => GetResourceString("SqlMisc_InvalidPrecScaleMessage");

	internal static string SqlMisc_FormatMessage => GetResourceString("SqlMisc_FormatMessage");

	internal static string SqlMisc_StreamErrorMessage => GetResourceString("SqlMisc_StreamErrorMessage");

	internal static string SqlMisc_TruncationMaxDataMessage => GetResourceString("SqlMisc_TruncationMaxDataMessage");

	internal static string SqlMisc_NotFilledMessage => GetResourceString("SqlMisc_NotFilledMessage");

	internal static string SqlMisc_AlreadyFilledMessage => GetResourceString("SqlMisc_AlreadyFilledMessage");

	internal static string SqlMisc_ClosedXmlReaderMessage => GetResourceString("SqlMisc_ClosedXmlReaderMessage");

	internal static string SqlMisc_InvalidOpStreamClosed => GetResourceString("SqlMisc_InvalidOpStreamClosed");

	internal static string SqlMisc_InvalidOpStreamNonWritable => GetResourceString("SqlMisc_InvalidOpStreamNonWritable");

	internal static string SqlMisc_InvalidOpStreamNonReadable => GetResourceString("SqlMisc_InvalidOpStreamNonReadable");

	internal static string SqlMisc_InvalidOpStreamNonSeekable => GetResourceString("SqlMisc_InvalidOpStreamNonSeekable");

	internal static string SqlMisc_SubclassMustOverride => GetResourceString("SqlMisc_SubclassMustOverride");

	internal static string SqlUdtReason_NoUdtAttribute => GetResourceString("SqlUdtReason_NoUdtAttribute");

	internal static string SQLUDT_InvalidSqlType => GetResourceString("SQLUDT_InvalidSqlType");

	internal static string Sql_InternalError => GetResourceString("Sql_InternalError");

	internal static string ADP_OperationAborted => GetResourceString("ADP_OperationAborted");

	internal static string ADP_OperationAbortedExceptionMessage => GetResourceString("ADP_OperationAbortedExceptionMessage");

	internal static string ADP_TransactionCompletedButNotDisposed => GetResourceString("ADP_TransactionCompletedButNotDisposed");

	internal static string SqlParameter_UnsupportedTVPOutputParameter => GetResourceString("SqlParameter_UnsupportedTVPOutputParameter");

	internal static string SqlParameter_DBNullNotSupportedForTVP => GetResourceString("SqlParameter_DBNullNotSupportedForTVP");

	internal static string SqlParameter_UnexpectedTypeNameForNonStruct => GetResourceString("SqlParameter_UnexpectedTypeNameForNonStruct");

	internal static string NullSchemaTableDataTypeNotSupported => GetResourceString("NullSchemaTableDataTypeNotSupported");

	internal static string InvalidSchemaTableOrdinals => GetResourceString("InvalidSchemaTableOrdinals");

	internal static string SQL_EnumeratedRecordMetaDataChanged => GetResourceString("SQL_EnumeratedRecordMetaDataChanged");

	internal static string SQL_EnumeratedRecordFieldCountChanged => GetResourceString("SQL_EnumeratedRecordFieldCountChanged");

	internal static string GT_Disabled => GetResourceString("GT_Disabled");

	internal static string SQL_UnknownSysTxIsolationLevel => GetResourceString("SQL_UnknownSysTxIsolationLevel");

	internal static string SQLNotify_AlreadyHasCommand => GetResourceString("SQLNotify_AlreadyHasCommand");

	internal static string SqlDependency_DatabaseBrokerDisabled => GetResourceString("SqlDependency_DatabaseBrokerDisabled");

	internal static string SqlDependency_DefaultOptionsButNoStart => GetResourceString("SqlDependency_DefaultOptionsButNoStart");

	internal static string SqlDependency_NoMatchingServerStart => GetResourceString("SqlDependency_NoMatchingServerStart");

	internal static string SqlDependency_NoMatchingServerDatabaseStart => GetResourceString("SqlDependency_NoMatchingServerDatabaseStart");

	internal static string SqlDependency_EventNoDuplicate => GetResourceString("SqlDependency_EventNoDuplicate");

	internal static string SqlDependency_IdMismatch => GetResourceString("SqlDependency_IdMismatch");

	internal static string SqlDependency_InvalidTimeout => GetResourceString("SqlDependency_InvalidTimeout");

	internal static string SqlDependency_DuplicateStart => GetResourceString("SqlDependency_DuplicateStart");

	internal static string SqlMetaData_InvalidSqlDbTypeForConstructorFormat => GetResourceString("SqlMetaData_InvalidSqlDbTypeForConstructorFormat");

	internal static string SqlMetaData_NameTooLong => GetResourceString("SqlMetaData_NameTooLong");

	internal static string SqlMetaData_SpecifyBothSortOrderAndOrdinal => GetResourceString("SqlMetaData_SpecifyBothSortOrderAndOrdinal");

	internal static string SqlProvider_InvalidDataColumnType => GetResourceString("SqlProvider_InvalidDataColumnType");

	internal static string SqlProvider_NotEnoughColumnsInStructuredType => GetResourceString("SqlProvider_NotEnoughColumnsInStructuredType");

	internal static string SqlProvider_DuplicateSortOrdinal => GetResourceString("SqlProvider_DuplicateSortOrdinal");

	internal static string SqlProvider_MissingSortOrdinal => GetResourceString("SqlProvider_MissingSortOrdinal");

	internal static string SqlProvider_SortOrdinalGreaterThanFieldCount => GetResourceString("SqlProvider_SortOrdinalGreaterThanFieldCount");

	internal static string SQLUDT_MaxByteSizeValue => GetResourceString("SQLUDT_MaxByteSizeValue");

	internal static string SQLUDT_Unexpected => GetResourceString("SQLUDT_Unexpected");

	internal static string SQLUDT_UnexpectedUdtTypeName => GetResourceString("SQLUDT_UnexpectedUdtTypeName");

	internal static string SQLUDT_InvalidUdtTypeName => GetResourceString("SQLUDT_InvalidUdtTypeName");

	internal static string SqlUdt_InvalidUdtMessage => GetResourceString("SqlUdt_InvalidUdtMessage");

	internal static string SQL_UDTTypeName => GetResourceString("SQL_UDTTypeName");

	internal static string SQL_InvalidUdt3PartNameFormat => GetResourceString("SQL_InvalidUdt3PartNameFormat");

	internal static string IEnumerableOfSqlDataRecordHasNoRows => GetResourceString("IEnumerableOfSqlDataRecordHasNoRows");

	internal static string SNI_ERROR_1 => GetResourceString("SNI_ERROR_1");

	internal static string SNI_ERROR_2 => GetResourceString("SNI_ERROR_2");

	internal static string SNI_ERROR_3 => GetResourceString("SNI_ERROR_3");

	internal static string SNI_ERROR_5 => GetResourceString("SNI_ERROR_5");

	internal static string SNI_ERROR_6 => GetResourceString("SNI_ERROR_6");

	internal static string SNI_ERROR_7 => GetResourceString("SNI_ERROR_7");

	internal static string SNI_ERROR_8 => GetResourceString("SNI_ERROR_8");

	internal static string SNI_ERROR_9 => GetResourceString("SNI_ERROR_9");

	internal static string SNI_ERROR_11 => GetResourceString("SNI_ERROR_11");

	internal static string SNI_ERROR_12 => GetResourceString("SNI_ERROR_12");

	internal static string SNI_ERROR_13 => GetResourceString("SNI_ERROR_13");

	internal static string SNI_ERROR_14 => GetResourceString("SNI_ERROR_14");

	internal static string SNI_ERROR_15 => GetResourceString("SNI_ERROR_15");

	internal static string SNI_ERROR_16 => GetResourceString("SNI_ERROR_16");

	internal static string SNI_ERROR_17 => GetResourceString("SNI_ERROR_17");

	internal static string SNI_ERROR_18 => GetResourceString("SNI_ERROR_18");

	internal static string SNI_ERROR_19 => GetResourceString("SNI_ERROR_19");

	internal static string SNI_ERROR_20 => GetResourceString("SNI_ERROR_20");

	internal static string SNI_ERROR_21 => GetResourceString("SNI_ERROR_21");

	internal static string SNI_ERROR_22 => GetResourceString("SNI_ERROR_22");

	internal static string SNI_ERROR_23 => GetResourceString("SNI_ERROR_23");

	internal static string SNI_ERROR_24 => GetResourceString("SNI_ERROR_24");

	internal static string SNI_ERROR_25 => GetResourceString("SNI_ERROR_25");

	internal static string SNI_ERROR_26 => GetResourceString("SNI_ERROR_26");

	internal static string SNI_ERROR_27 => GetResourceString("SNI_ERROR_27");

	internal static string SNI_ERROR_28 => GetResourceString("SNI_ERROR_28");

	internal static string SNI_ERROR_29 => GetResourceString("SNI_ERROR_29");

	internal static string SNI_ERROR_30 => GetResourceString("SNI_ERROR_30");

	internal static string SNI_ERROR_31 => GetResourceString("SNI_ERROR_31");

	internal static string SNI_ERROR_32 => GetResourceString("SNI_ERROR_32");

	internal static string SNI_ERROR_33 => GetResourceString("SNI_ERROR_33");

	internal static string SNI_ERROR_34 => GetResourceString("SNI_ERROR_34");

	internal static string SNI_ERROR_35 => GetResourceString("SNI_ERROR_35");

	internal static string SNI_ERROR_36 => GetResourceString("SNI_ERROR_36");

	internal static string SNI_ERROR_37 => GetResourceString("SNI_ERROR_37");

	internal static string SNI_ERROR_38 => GetResourceString("SNI_ERROR_38");

	internal static string SNI_ERROR_39 => GetResourceString("SNI_ERROR_39");

	internal static string SNI_ERROR_40 => GetResourceString("SNI_ERROR_40");

	internal static string SNI_ERROR_41 => GetResourceString("SNI_ERROR_41");

	internal static string SNI_ERROR_42 => GetResourceString("SNI_ERROR_42");

	internal static string SNI_ERROR_43 => GetResourceString("SNI_ERROR_43");

	internal static string SNI_ERROR_44 => GetResourceString("SNI_ERROR_44");

	internal static string SNI_ERROR_47 => GetResourceString("SNI_ERROR_47");

	internal static string SNI_ERROR_48 => GetResourceString("SNI_ERROR_48");

	internal static string SNI_ERROR_49 => GetResourceString("SNI_ERROR_49");

	internal static string SNI_ERROR_50 => GetResourceString("SNI_ERROR_50");

	internal static string SNI_ERROR_51 => GetResourceString("SNI_ERROR_51");

	internal static string SNI_ERROR_52 => GetResourceString("SNI_ERROR_52");

	internal static string SNI_ERROR_53 => GetResourceString("SNI_ERROR_53");

	internal static string SNI_ERROR_54 => GetResourceString("SNI_ERROR_54");

	internal static string SNI_ERROR_55 => GetResourceString("SNI_ERROR_55");

	internal static string SNI_ERROR_56 => GetResourceString("SNI_ERROR_56");

	internal static string SNI_ERROR_57 => GetResourceString("SNI_ERROR_57");

	internal static string Snix_Connect => GetResourceString("Snix_Connect");

	internal static string Snix_PreLoginBeforeSuccessfulWrite => GetResourceString("Snix_PreLoginBeforeSuccessfulWrite");

	internal static string Snix_PreLogin => GetResourceString("Snix_PreLogin");

	internal static string Snix_LoginSspi => GetResourceString("Snix_LoginSspi");

	internal static string Snix_Login => GetResourceString("Snix_Login");

	internal static string Snix_EnableMars => GetResourceString("Snix_EnableMars");

	internal static string Snix_AutoEnlist => GetResourceString("Snix_AutoEnlist");

	internal static string Snix_GetMarsSession => GetResourceString("Snix_GetMarsSession");

	internal static string Snix_Execute => GetResourceString("Snix_Execute");

	internal static string Snix_Read => GetResourceString("Snix_Read");

	internal static string Snix_Close => GetResourceString("Snix_Close");

	internal static string Snix_SendRows => GetResourceString("Snix_SendRows");

	internal static string Snix_ProcessSspi => GetResourceString("Snix_ProcessSspi");

	internal static string LocalDB_FailedGetDLLHandle => GetResourceString("LocalDB_FailedGetDLLHandle");

	internal static string LocalDB_MethodNotFound => GetResourceString("LocalDB_MethodNotFound");

	internal static string LocalDB_UnobtainableMessage => GetResourceString("LocalDB_UnobtainableMessage");

	internal static string SQLROR_RecursiveRoutingNotSupported => GetResourceString("SQLROR_RecursiveRoutingNotSupported");

	internal static string SQLROR_FailoverNotSupported => GetResourceString("SQLROR_FailoverNotSupported");

	internal static string SQLROR_UnexpectedRoutingInfo => GetResourceString("SQLROR_UnexpectedRoutingInfo");

	internal static string SQLROR_InvalidRoutingInfo => GetResourceString("SQLROR_InvalidRoutingInfo");

	internal static string SQLROR_TimeoutAfterRoutingInfo => GetResourceString("SQLROR_TimeoutAfterRoutingInfo");

	internal static string SQLCR_InvalidConnectRetryCountValue => GetResourceString("SQLCR_InvalidConnectRetryCountValue");

	internal static string SQLCR_InvalidConnectRetryIntervalValue => GetResourceString("SQLCR_InvalidConnectRetryIntervalValue");

	internal static string SQLCR_NextAttemptWillExceedQueryTimeout => GetResourceString("SQLCR_NextAttemptWillExceedQueryTimeout");

	internal static string SQLCR_EncryptionChanged => GetResourceString("SQLCR_EncryptionChanged");

	internal static string SQLCR_TDSVestionNotPreserved => GetResourceString("SQLCR_TDSVestionNotPreserved");

	internal static string SQLCR_AllAttemptsFailed => GetResourceString("SQLCR_AllAttemptsFailed");

	internal static string SQLCR_UnrecoverableServer => GetResourceString("SQLCR_UnrecoverableServer");

	internal static string SQLCR_UnrecoverableClient => GetResourceString("SQLCR_UnrecoverableClient");

	internal static string SQLCR_NoCRAckAtReconnection => GetResourceString("SQLCR_NoCRAckAtReconnection");

	internal static string SQL_UnsupportedKeyword => GetResourceString("SQL_UnsupportedKeyword");

	internal static string SQL_UnsupportedFeature => GetResourceString("SQL_UnsupportedFeature");

	internal static string SQL_UnsupportedToken => GetResourceString("SQL_UnsupportedToken");

	internal static string SQL_DbTypeNotSupportedOnThisPlatform => GetResourceString("SQL_DbTypeNotSupportedOnThisPlatform");

	internal static string SQL_NetworkLibraryNotSupported => GetResourceString("SQL_NetworkLibraryNotSupported");

	internal static string SNI_PN0 => GetResourceString("SNI_PN0");

	internal static string SNI_PN1 => GetResourceString("SNI_PN1");

	internal static string SNI_PN2 => GetResourceString("SNI_PN2");

	internal static string SNI_PN3 => GetResourceString("SNI_PN3");

	internal static string SNI_PN4 => GetResourceString("SNI_PN4");

	internal static string SNI_PN5 => GetResourceString("SNI_PN5");

	internal static string SNI_PN6 => GetResourceString("SNI_PN6");

	internal static string SNI_PN7 => GetResourceString("SNI_PN7");

	internal static string SNI_PN8 => GetResourceString("SNI_PN8");

	internal static string SNI_PN9 => GetResourceString("SNI_PN9");

	internal static string AZURESQL_GenericEndpoint => GetResourceString("AZURESQL_GenericEndpoint");

	internal static string AZURESQL_GermanEndpoint => GetResourceString("AZURESQL_GermanEndpoint");

	internal static string AZURESQL_UsGovEndpoint => GetResourceString("AZURESQL_UsGovEndpoint");

	internal static string AZURESQL_ChinaEndpoint => GetResourceString("AZURESQL_ChinaEndpoint");

	internal static string net_gssapi_operation_failed_detailed => GetResourceString("net_gssapi_operation_failed_detailed");

	internal static string net_gssapi_operation_failed => GetResourceString("net_gssapi_operation_failed");

	internal static string net_gssapi_operation_failed_detailed_majoronly => GetResourceString("net_gssapi_operation_failed_detailed_majoronly");

	internal static string net_gssapi_operation_failed_majoronly => GetResourceString("net_gssapi_operation_failed_majoronly");

	internal static string net_gssapi_ntlm_missing_plugin => GetResourceString("net_gssapi_ntlm_missing_plugin");

	internal static string net_ntlm_not_possible_default_cred => GetResourceString("net_ntlm_not_possible_default_cred");

	internal static string net_nego_not_supported_empty_target_with_defaultcreds => GetResourceString("net_nego_not_supported_empty_target_with_defaultcreds");

	internal static string net_nego_server_not_supported => GetResourceString("net_nego_server_not_supported");

	internal static string net_nego_protection_level_not_supported => GetResourceString("net_nego_protection_level_not_supported");

	internal static string net_context_buffer_too_small => GetResourceString("net_context_buffer_too_small");

	internal static string net_auth_message_not_encrypted => GetResourceString("net_auth_message_not_encrypted");

	internal static string net_securitypackagesupport => GetResourceString("net_securitypackagesupport");

	internal static string net_log_operation_failed_with_error => GetResourceString("net_log_operation_failed_with_error");

	internal static string net_MethodNotImplementedException => GetResourceString("net_MethodNotImplementedException");

	internal static string event_OperationReturnedSomething => GetResourceString("event_OperationReturnedSomething");

	internal static string net_invalid_enum => GetResourceString("net_invalid_enum");

	internal static string SSPIInvalidHandleType => GetResourceString("SSPIInvalidHandleType");

	internal static string LocalDBNotSupported => GetResourceString("LocalDBNotSupported");

	internal static string PlatformNotSupported_DataSqlClient => GetResourceString("PlatformNotSupported_DataSqlClient");

	internal static string SqlParameter_InvalidTableDerivedPrecisionForTvp => GetResourceString("SqlParameter_InvalidTableDerivedPrecisionForTvp");

	internal static string SqlProvider_InvalidDataColumnMaxLength => GetResourceString("SqlProvider_InvalidDataColumnMaxLength");

	internal static string MDF_InvalidXmlInvalidValue => GetResourceString("MDF_InvalidXmlInvalidValue");

	internal static string MDF_CollectionNameISNotUnique => GetResourceString("MDF_CollectionNameISNotUnique");

	internal static string MDF_InvalidXmlMissingColumn => GetResourceString("MDF_InvalidXmlMissingColumn");

	internal static string MDF_InvalidXml => GetResourceString("MDF_InvalidXml");

	internal static string MDF_NoColumns => GetResourceString("MDF_NoColumns");

	internal static string MDF_QueryFailed => GetResourceString("MDF_QueryFailed");

	internal static string MDF_TooManyRestrictions => GetResourceString("MDF_TooManyRestrictions");

	internal static string MDF_DataTableDoesNotExist => GetResourceString("MDF_DataTableDoesNotExist");

	internal static string MDF_UndefinedCollection => GetResourceString("MDF_UndefinedCollection");

	internal static string MDF_UnsupportedVersion => GetResourceString("MDF_UnsupportedVersion");

	internal static string MDF_MissingRestrictionColumn => GetResourceString("MDF_MissingRestrictionColumn");

	internal static string MDF_MissingRestrictionRow => GetResourceString("MDF_MissingRestrictionRow");

	internal static string MDF_IncorrectNumberOfDataSourceInformationRows => GetResourceString("MDF_IncorrectNumberOfDataSourceInformationRows");

	internal static string MDF_MissingDataSourceInformationColumn => GetResourceString("MDF_MissingDataSourceInformationColumn");

	internal static string MDF_AmbigousCollectionName => GetResourceString("MDF_AmbigousCollectionName");

	internal static string MDF_UnableToBuildCollection => GetResourceString("MDF_UnableToBuildCollection");

	internal static string ADP_InvalidArgumentLength => GetResourceString("ADP_InvalidArgumentLength");

	internal static string ADP_MustBeReadOnly => GetResourceString("ADP_MustBeReadOnly");

	internal static string ADP_InvalidMixedUsageOfSecureAndClearCredential => GetResourceString("ADP_InvalidMixedUsageOfSecureAndClearCredential");

	internal static string ADP_InvalidMixedUsageOfSecureCredentialAndIntegratedSecurity => GetResourceString("ADP_InvalidMixedUsageOfSecureCredentialAndIntegratedSecurity");

	internal static string SQL_ChangePasswordArgumentMissing => GetResourceString("SQL_ChangePasswordArgumentMissing");

	internal static string SQL_ChangePasswordConflictsWithSSPI => GetResourceString("SQL_ChangePasswordConflictsWithSSPI");

	internal static string SQL_ChangePasswordRequiresYukon => GetResourceString("SQL_ChangePasswordRequiresYukon");

	internal static string SQL_ChangePasswordUseOfUnallowedKey => GetResourceString("SQL_ChangePasswordUseOfUnallowedKey");

	internal static string SQL_ParsingErrorWithState => GetResourceString("SQL_ParsingErrorWithState");

	internal static string SQL_ParsingErrorValue => GetResourceString("SQL_ParsingErrorValue");

	internal static string ADP_InvalidMixedUsageOfAccessTokenAndIntegratedSecurity => GetResourceString("ADP_InvalidMixedUsageOfAccessTokenAndIntegratedSecurity");

	internal static string ADP_InvalidMixedUsageOfAccessTokenAndUserIDPassword => GetResourceString("ADP_InvalidMixedUsageOfAccessTokenAndUserIDPassword");

	internal static string ADP_InvalidMixedUsageOfCredentialAndAccessToken => GetResourceString("ADP_InvalidMixedUsageOfCredentialAndAccessToken");

	internal static string SQL_ParsingErrorFeatureId => GetResourceString("SQL_ParsingErrorFeatureId");

	internal static string SQL_ParsingErrorAuthLibraryType => GetResourceString("SQL_ParsingErrorAuthLibraryType");

	internal static string SqlFileStream_InvalidPath => GetResourceString("SqlFileStream_InvalidPath");

	internal static string SqlFileStream_PathNotValidDiskResource => GetResourceString("SqlFileStream_PathNotValidDiskResource");

	internal static string SqlFileStream_FileAlreadyInTransaction => GetResourceString("SqlFileStream_FileAlreadyInTransaction");

	internal static string SqlFileStream_InvalidParameter => GetResourceString("SqlFileStream_InvalidParameter");

	internal static string SqlFileStream_NotSupported => GetResourceString("SqlFileStream_NotSupported");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey, string defaultString = null)
	{
		if (UsingResourceKeys())
		{
			return defaultString ?? resourceKey;
		}
		string text = null;
		try
		{
			text = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		if (defaultString != null && resourceKey.Equals(text))
		{
			return defaultString;
		}
		return text;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, p1, p2, p3);
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + ", " + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(provider, resourceFormat, p1);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(provider, resourceFormat, p1, p2);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(provider, resourceFormat, p1, p2, p3);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + ", " + string.Join(", ", args);
			}
			return string.Format(provider, resourceFormat, args);
		}
		return resourceFormat;
	}
}
