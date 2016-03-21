// Copyright ?2004, 2010, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

namespace MySql.Data.MySqlClient
{
  /// <summary>
  /// Collection of error codes that can be returned by the server
  /// </summary>
  public class MySqlError
  {
    private string level;
    private int code;
    private string message;

    /// <summary></summary>
    /// <param name="level"></param>
    /// <param name="code"></param>
    /// <param name="message"></param>
    public MySqlError(string level, int code, string message)
    {
      this.level = level;
      this.code = code;
      this.message = message;
    }

    /// <summary>
    /// Error level
    /// </summary>
    public string Level
    {
      get { return level; }
    }

    /// <summary>
    /// Error code
    /// </summary>
    public int Code
    {
      get { return code; }
    }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message
    {
      get { return message; }
    }
  };

  /// <summary>
  /// Provides a reference to error codes returned by MySQL.
  /// </summary>
  public enum MySqlErrorCode
  {
    None = 0,
    ///<summary></summary>
    ///<remarks>ER_HASHCHK</remarks>
    HashCheck = 1000,
    ///<summary></summary>
    ///<remarks>ER_NISAMCHK</remarks>
    ISAMCheck = 1001,
    ///<summary></summary>
    ///<remarks>ER_NO</remarks>
    No = 1002,
    ///<summary></summary>
    ///<remarks>ER_YES</remarks>
    Yes = 1003,
    ///<summary>The file couldn't be created.</summary>
    ///<remarks>ER_CANT_CREATE_FILE</remarks>
    CannotCreateFile = 1004,
    ///<summary>The table couldn't be created.</summary>
    ///<remarks>ER_CANT_CREATE_TABLE</remarks>
    CannotCreateTable = 1005,
    ///<summary>The database couldn't be created.</summary>
    ///<remarks>ER_CANT_CREATE_DB</remarks>
    CannotCreateDatabase = 1006,
    ///<summary>The database couldn't be created, it already exists.</summary>
    ///<remarks>ER_DB_CREATE_EXISTS</remarks>
    DatabaseCreateExists = 1007,
    ///<summary>The database couldn't be dropped, it doesn't exist.</summary>
    ///<remarks>ER_DB_DROP_EXISTS</remarks>
    DatabaseDropExists = 1008,
    ///<summary>The database couldn't be dropped, the file can't be deleted.</summary>
    ///<remarks>ER_DB_DROP_DELETE</remarks>
    DatabaseDropDelete = 1009,
    ///<summary>The database couldn't be dropped, the directory can't be deleted.</summary>
    ///<remarks>ER_DB_DROP_RMDIR</remarks>
    DatabaseDropRemoveDir = 1010,
    ///<summary>The file couldn't be deleted.</summary>
    ///<remarks>ER_CANT_DELETE_FILE</remarks>
    CannotDeleteFile = 1011,
    ///<summary>The record couldn't be read from the system table.</summary>
    ///<remarks>ER_CANT_FIND_SYSTEM_REC</remarks>
    CannotFindSystemRecord = 1012,
    ///<summary>The status couldn't be retrieved.</summary>
    ///<remarks>ER_CANT_GET_STAT</remarks>
    CannotGetStatus = 1013,
    ///<summary>The working directory couldn't be retrieved.</summary>
    ///<remarks>ER_CANT_GET_WD</remarks>
    CannotGetWorkingDirectory = 1014,
    ///<summary>The file couldn't be locked.</summary>
    ///<remarks>ER_CANT_LOCK</remarks>
    CannotLock = 1015,
    ///<summary>The file couldn't be opened.</summary>
    ///<remarks>ER_CANT_OPEN_FILE</remarks>
    CannotOpenFile = 1016,
    ///<summary>The file couldn't be found.</summary>
    ///<remarks>ER_FILE_NOT_FOUND</remarks>
    FileNotFound = 1017,
    ///<summary>The directory couldn't be read.</summary>
    ///<remarks>ER_CANT_READ_DIR</remarks>
    CannotReadDirectory = 1018,
    ///<summary>The working directory couldn't be entered.</summary>
    ///<remarks>ER_CANT_SET_WD</remarks>
    CannotSetWorkingDirectory = 1019,
    ///<summary>The record changed since it was last read.</summary>
    ///<remarks>ER_CHECKREAD</remarks>
    CheckRead = 1020,
    ///<summary>The disk is full.</summary>
    ///<remarks>ER_DISK_FULL</remarks>
    DiskFull = 1021,
    /// <summary>
    /// There is already a key with the given values.
    /// </summary>
    DuplicateKey = 1022,
    ///<summary>An error occurred when closing the file.</summary>
    ///<remarks>ER_ERROR_ON_CLOSE</remarks>
    ErrorOnClose = 1023,
    ///<summary>An error occurred when reading from the file.</summary>
    ///<remarks>ER_ERROR_ON_READ</remarks>
    ErrorOnRead = 1024,
    ///<summary>An error occurred when renaming then file.</summary>
    ///<remarks>ER_ERROR_ON_RENAME</remarks>
    ErrorOnRename = 1025,
    ///<summary>An error occurred when writing to the file.</summary>
    ///<remarks>ER_ERROR_ON_WRITE</remarks>
    ErrorOnWrite = 1026,
    ///<summary>The file is in use.</summary>
    ///<remarks>ER_FILE_USED</remarks>
    FileUsed = 1027,
    ///<summary>Sorting has been aborted.</summary>
    ///<remarks>ER_FILSORT_ABORT</remarks>
    FileSortAborted = 1028,
    ///<summary>The view doesn't exist.</summary>
    ///<remarks>ER_FORM_NOT_FOUND</remarks>
    FormNotFound = 1029,
    ///<summary>Got the specified error from the table storage engine.</summary>
    ///<remarks>ER_GET_ERRNO</remarks>
    GetErrorNumber = 1030,
    ///<summary>The table storage engine doesn't support the specified option.</summary>
    ///<remarks>ER_ILLEGAL_HA</remarks>
    IllegalHA = 1031,
    /// <summary>
    /// The specified key was not found.
    /// </summary>
    KeyNotFound = 1032,
    ///<summary>The file contains incorrect information.</summary>
    ///<remarks>ER_NOT_FORM_FILE</remarks>
    NotFormFile = 1033,
    ///<summary>The key file is incorrect for the table, it should be repaired.</summary>
    ///<remarks>ER_NOT_KEYFILE</remarks>
    NotKeyFile = 1034,
    ///<summary>The key file is old for the table, it should be repaired.</summary>
    ///<remarks>ER_OLD_KEYFILE</remarks>
    OldKeyFile = 1035,
    ///<summary>The table is read-only</summary>
    ///<remarks>ER_OPEN_AS_READONLY</remarks>
    OpenAsReadOnly = 1036,
    ///<summary>The server is out of memory, it should be restarted.</summary>
    ///<remarks>ER_OUTOFMEMORY</remarks>
    OutOfMemory = 1037,
    ///<summary>The server is out of sort-memory, the sort buffer size should be increased.</summary>
    ///<remarks>ER_OUT_OF_SORTMEMORY</remarks>
    OutOfSortMemory = 1038,
    ///<summary>An unexpected EOF was found when reading from the file.</summary>
    ///<remarks>ER_UNEXPECTED_EOF</remarks>
    UnexepectedEOF = 1039,
    ///<summary>Too many connections are open.</summary>
    ///<remarks>ER_CON_COUNT_ERROR</remarks>
    ConnectionCountError = 1040,
    ///<summary>The server is out of resources, check if MySql or some other process is using all available memory.</summary>
    ///<remarks>ER_OUT_OF_RESOURCES</remarks>
    OutOfResources = 1041,
    /// <summary>
    /// Given when the connection is unable to successfully connect to host.
    /// </summary>
    UnableToConnectToHost = 1042,
    ///<summary>The handshake was invalid.</summary>
    ///<remarks>ER_HANDSHAKE_ERROR</remarks>
    HandshakeError = 1043,
    ///<summary>Access was denied for the specified user using the specified database.</summary>
    ///<remarks>ER_DBACCESS_DENIED_ERROR</remarks>
    DatabaseAccessDenied = 1044,
    /// <summary>
    /// Normally returned when an incorrect password is given
    /// </summary>
    AccessDenied = 1045,
    ///<summary>No database has been selected.</summary>
    ///<remarks>ER_NO_DB_ERROR</remarks>
    NoDatabaseSelected = 1046,
    ///<summary>The command is unknown.</summary>
    ///<remarks>ER_UNKNOWN_COM_ERROR</remarks>
    UnknownCommand = 1047,
    ///<summary>The specified column cannot be NULL.</summary>
    ///<remarks>ER_BAD_NULL_ERROR</remarks>
    ColumnCannotBeNull = 1048,
    /// <summary>The specified database is not known.</summary>
    UnknownDatabase = 1049,
    ///<summary>The specified table already exists.</summary>
    ///<remarks>ER_TABLE_EXISTS_ERROR</remarks>
    TableExists = 1050,
    ///<summary>The specified table is unknown.</summary>
    ///<remarks>ER_BAD_TABLE_ERROR</remarks>
    BadTable = 1051,
    ///<summary>The specified column is ambiguous.</summary>
    ///<remarks>ER_NON_UNIQ_ERROR</remarks>
    NonUnique = 1052,
    ///<summary>The server is currently being shutdown.</summary>
    ///<remarks>ER_SERVER_SHUTDOWN</remarks>
    ServerShutdown = 1053,
    ///<summary>The specified columns is unknown.</summary>
    ///<remarks>ER_BAD_FIELD_ERROR</remarks>
    BadFieldError = 1054,
    ///<summary>The specified column isn't in GROUP BY.</summary>
    ///<remarks>ER_WRONG_FIELD_WITH_GROUP</remarks>
    WrongFieldWithGroup = 1055,
    ///<summary>The specified columns cannot be grouped on.</summary>
    ///<remarks>ER_WRONG_GROUP_FIELD</remarks>
    WrongGroupField = 1056,
    ///<summary>There are sum functions and columns in the same statement.</summary>
    ///<remarks>ER_WRONG_SUM_SELECT</remarks>
    WrongSumSelected = 1057,
    ///<summary>The column count doesn't match the value count.</summary>
    ///<remarks>ER_WRONG_VALUE_COUNT</remarks>
    WrongValueCount = 1058,
    ///<summary>The identifier name is too long.</summary>
    ///<remarks>ER_TOO_LONG_IDENT</remarks>
    TooLongIdentifier = 1059,
    ///<summary>The column name is duplicated.</summary>
    ///<remarks>ER_DUP_FIELDNAME</remarks>
    DuplicateFieldName = 1060,
    /// <summary>
    /// Duplicate Key Name
    /// </summary>
    DuplicateKeyName = 1061,
    /// <summary>
    /// Duplicate Key Entry
    /// </summary>
    DuplicateKeyEntry = 1062,
    ///<summary>The column specifier is incorrect.</summary>
    ///<remarks>ER_WRONG_FIELD_SPEC</remarks>
    WrongFieldSpecifier = 1063,
    ///<summary>An error occurred when parsing the statement.</summary>
    ///<remarks>ER_PARSE_ERROR</remarks>
    ParseError = 1064,
    ///<summary>The statement is empty.</summary>
    ///<remarks>ER_EMPTY_QUERY</remarks>
    EmptyQuery = 1065,
    ///<summary>The table alias isn't unique.</summary>
    ///<remarks>ER_NONUNIQ_TABLE</remarks>
    NonUniqueTable = 1066,
    ///<summary>The default value is invalid for the specified field.</summary>
    ///<remarks>ER_INVALID_DEFAULT</remarks>
    InvalidDefault = 1067,
    ///<summary>The table has multiple primary keys defined.</summary>
    ///<remarks>ER_MULTIPLE_PRI_KEY</remarks>
    MultiplePrimaryKey = 1068,
    ///<summary>Too many keys were defined for the table.</summary>
    ///<remarks>ER_TOO_MANY_KEYS</remarks>
    TooManyKeys = 1069,
    ///<summary>Too many parts to the keys were defined for the table.</summary>
    ///<remarks>ER_TOO_MANY_KEY_PARTS</remarks>
    TooManyKeysParts = 1070,
    ///<summary>The specified key is too long</summary>
    ///<remarks>ER_TOO_LONG_KEY</remarks>
    TooLongKey = 1071,
    ///<summary>The specified key column doesn't exist in the table.</summary>
    ///<remarks>ER_KEY_COLUMN_DOES_NOT_EXITS</remarks>
    KeyColumnDoesNotExist = 1072,
    ///<summary>The BLOB column was used as a key, this can't be done.</summary>
    ///<remarks>ER_BLOB_USED_AS_KEY</remarks>
    BlobUsedAsKey = 1073,
    ///<summary>The column length is too big for the specified column type.</summary>
    ///<remarks>ER_TOO_BIG_FIELDLENGTH</remarks>
    TooBigFieldLength = 1074,
    ///<summary>There can only be one auto-column, and it must be defined as a PK.</summary>
    ///<remarks>ER_WRONG_AUTO_KEY</remarks>
    WrongAutoKey = 1075,
    ///<summary>The server is ready to accept connections.</summary>
    ///<remarks>ER_READY</remarks>
    Ready = 1076,
    ///<summary></summary>
    ///<remarks>ER_NORMAL_SHUTDOWN</remarks>
    NormalShutdown = 1077,
    ///<summary>The server received the specified signal and is aborting.</summary>
    ///<remarks>ER_GOT_SIGNAL</remarks>
    GotSignal = 1078,
    ///<summary>The server shutdown is complete.</summary>
    ///<remarks>ER_SHUTDOWN_COMPLETE</remarks>
    ShutdownComplete = 1079,
    ///<summary>The server is forcing close of the specified thread.</summary>
    ///<remarks>ER_FORCING_CLOSE</remarks>
    ForcingClose = 1080,
    ///<summary>An error occurred when creating the IP socket.</summary>
    ///<remarks>ER_IPSOCK_ERROR</remarks>
    IPSocketError = 1081,
    ///<summary>The table has no index like the one used in CREATE INDEX.</summary>
    ///<remarks>ER_NO_SUCH_INDEX</remarks>
    NoSuchIndex = 1082,
    ///<summary>The field separator argument is not what is expected, check the manual.</summary>
    ///<remarks>ER_WRONG_FIELD_TERMINATORS</remarks>
    WrongFieldTerminators = 1083,
    ///<summary>The BLOB columns must terminated, fixed row lengths cannot be used.</summary>
    ///<remarks>ER_BLOBS_AND_NO_TERMINATED</remarks>
    BlobsAndNoTerminated = 1084,
    ///<summary>The text file cannot be read.</summary>
    ///<remarks>ER_TEXTFILE_NOT_READABLE</remarks>
    TextFileNotReadable = 1085,
    ///<summary>The specified file already exists.</summary>
    ///<remarks>ER_FILE_EXISTS_ERROR</remarks>
    FileExists = 1086,
    ///<summary>Information returned by the LOAD statement.</summary>
    ///<remarks>ER_LOAD_INFO</remarks>
    LoadInfo = 1087,
    ///<summary>Information returned by an UPDATE statement.</summary>
    ///<remarks>ER_ALTER_INFO</remarks>
    AlterInfo = 1088,
    ///<summary>The prefix key is incorrect.</summary>
    ///<remarks>ER_WRONG_SUB_KEY</remarks>
    WrongSubKey = 1089,
    ///<summary>All columns cannot be removed from a table, use DROP TABLE instead.</summary>
    ///<remarks>ER_CANT_REMOVE_ALL_FIELDS</remarks>
    CannotRemoveAllFields = 1090,
    ///<summary>Cannot DROP, check that the column or key exists.</summary>
    ///<remarks>ER_CANT_DROP_FIELD_OR_KEY</remarks>
    CannotDropFieldOrKey = 1091,
    ///<summary>Information returned by an INSERT statement.</summary>
    ///<remarks>ER_INSERT_INFO</remarks>
    InsertInfo = 1092,
    ///<summary>The target table cannot be specified for update in FROM clause.</summary>
    ///<remarks>ER_UPDATE_TABLE_USED</remarks>
    UpdateTableUsed = 1093,
    ///<summary>The specified thread ID is unknown.</summary>
    ///<remarks>ER_NO_SUCH_THREAD</remarks>
    NoSuchThread = 1094,
    ///<summary>The thread cannot be killed, the current user is not the owner.</summary>
    ///<remarks>ER_KILL_DENIED_ERROR</remarks>
    KillDenied = 1095,
    ///<summary>No tables used in the statement.</summary>
    ///<remarks>ER_NO_TABLES_USED</remarks>
    NoTablesUsed = 1096,
    ///<summary>Too many string have been used for the specified column and SET.</summary>
    ///<remarks>ER_TOO_BIG_SET</remarks>
    TooBigSet = 1097,
    ///<summary>A unique filename couldn't be generated.</summary>
    ///<remarks>ER_NO_UNIQUE_LOGFILE</remarks>
    NoUniqueLogFile = 1098,
    ///<summary>The specified table was locked with a READ lock, and can't be updated.</summary>
    ///<remarks>ER_TABLE_NOT_LOCKED_FOR_WRITE</remarks>
    TableNotLockedForWrite = 1099,
    ///<summary>The specified table was not locked with LOCK TABLES.</summary>
    ///<remarks>ER_TABLE_NOT_LOCKED</remarks>
    TableNotLocked = 1100,
    ///<summary>BLOB and Text columns cannot have a default value.</summary>
    ///<remarks>ER_BLOB_CANT_HAVE_DEFAULT</remarks>
    BlobCannotHaveDefault = 1101,
    ///<summary>The specified database name is incorrect.</summary>
    ///<remarks>ER_WRONG_DB_NAME</remarks>
    WrongDatabaseName = 1102,
    ///<summary>The specified table name is incorrect.</summary>
    ///<remarks>ER_WRONG_TABLE_NAME</remarks>
    WrongTableName = 1103,
    ///<summary>The SELECT command would examine more than MAX_JOIN_SIZE rows, check the WHERE clause and use SET SQL_BIG_SELECTS=1 or SET SQL_MAX_JOIN_SIZE=# if the SELECT is ok.</summary>
    ///<remarks>ER_TOO_BIG_SELECT</remarks>
    TooBigSelect = 1104,
    ///<summary>An unknown error occurred.</summary>
    ///<remarks>ER_UNKNOWN_ERROR</remarks>
    UnknownError = 1105,
    ///<summary>The specified procedure is unknown.</summary>
    ///<remarks>ER_UNKNOWN_PROCEDURE</remarks>
    UnknownProcedure = 1106,
    ///<summary>The number of parameters provided for the specified procedure is incorrect.</summary>
    ///<remarks>ER_WRONG_PARAMCOUNT_TO_PROCEDURE</remarks>
    WrongParameterCountToProcedure = 1107,
    ///<summary>The parameters provided for the specified procedure are incorrect.</summary>
    ///<remarks>ER_WRONG_PARAMETERS_TO_PROCEDURE</remarks>
    WrongParametersToProcedure = 1108,
    ///<summary>The specified table is unknown.</summary>
    ///<remarks>ER_UNKNOWN_TABLE</remarks>
    UnknownTable = 1109,
    ///<summary>The specified column has been specified twice.</summary>
    ///<remarks>ER_FIELD_SPECIFIED_TWICE</remarks>
    FieldSpecifiedTwice = 1110,
    ///<summary>The group function has been incorrectly used.</summary>
    ///<remarks>ER_INVALID_GROUP_FUNC_USE</remarks>
    InvalidGroupFunctionUse = 1111,
    ///<summary>The specified table uses an extension that doesn't exist in this MySQL version.</summary>
    ///<remarks>ER_UNSUPPORTED_EXTENSION</remarks>
    UnsupportedExtenstion = 1112,
    ///<summary>The table must have at least one column.</summary>
    ///<remarks>ER_TABLE_MUST_HAVE_COLUMNS</remarks>
    TableMustHaveColumns = 1113,
    ///<summary>The specified table is full.</summary>
    ///<remarks>ER_RECORD_FILE_FULL</remarks>
    RecordFileFull = 1114,
    ///<summary>The specified character set is unknown.</summary>
    ///<remarks>ER_UNKNOWN_CHARACTER_SET</remarks>
    UnknownCharacterSet = 1115,
    ///<summary>Too many tables, MySQL can only use the specified number of tables in a JOIN.</summary>
    ///<remarks>ER_TOO_MANY_TABLES</remarks>
    TooManyTables = 1116,
    ///<summary>Too many columns</summary>
    ///<remarks>ER_TOO_MANY_FIELDS</remarks>
    TooManyFields = 1117,
    ///<summary>The row size is too large, the maximum row size for the used tables (not counting BLOBS) is specified, change some columns or BLOBS.</summary>
    ///<remarks>ER_TOO_BIG_ROWSIZE</remarks>
    TooBigRowSize = 1118,
    ///<summary>A thread stack overrun occurred. Stack statistics are specified.</summary>
    ///<remarks>ER_STACK_OVERRUN</remarks>
    StackOverrun = 1119,
    ///<summary>A cross dependency was found in the OUTER JOIN, examine the ON conditions.</summary>
    ///<remarks>ER_WRONG_OUTER_JOIN</remarks>
    WrongOuterJoin = 1120,
    ///<summary>The table handler doesn't support NULL in the given index, change specified column to be NOT NULL or use another handler.</summary>
    ///<remarks>ER_NULL_COLUMN_IN_INDEX</remarks>
    NullColumnInIndex = 1121,
    ///<summary>The specified user defined function cannot be loaded.</summary>
    ///<remarks>ER_CANT_FIND_UDF</remarks>
    CannotFindUDF = 1122,
    ///<summary>The specified user defined function cannot be initialised.</summary>
    ///<remarks>ER_CANT_INITIALIZE_UDF</remarks>
    CannotInitializeUDF = 1123,
    ///<summary>No paths are allowed for the shared library.</summary>
    ///<remarks>ER_UDF_NO_PATHS</remarks>
    UDFNoPaths = 1124,
    ///<summary>The specified user defined function already exists.</summary>
    ///<remarks>ER_UDF_EXISTS</remarks>
    UDFExists = 1125,
    ///<summary>The specified shared library cannot be opened.</summary>
    ///<remarks>ER_CANT_OPEN_LIBRARY</remarks>
    CannotOpenLibrary = 1126,
    ///<summary>The specified symbol cannot be found in the library.</summary>
    ///<remarks>ER_CANT_FIND_DL_ENTRY</remarks>
    CannotFindDLEntry = 1127,
    ///<summary>The specified function is not defined.</summary>
    ///<remarks>ER_FUNCTION_NOT_DEFINED</remarks>
    FunctionNotDefined = 1128,
    ///<summary>The specified host is blocked because of too many connection errors, unblock with 'mysqladmin flush-hosts'.</summary>
    ///<remarks>ER_HOST_IS_BLOCKED</remarks>
    HostIsBlocked = 1129,
    /// <summary>
    /// The given host is not allowed to connect
    /// </summary>
    HostNotPrivileged = 1130,
    /// <summary>
    /// The anonymous user is not allowed to connect
    /// </summary>
    AnonymousUser = 1131,
    /// <summary>
    /// The given password is not allowed
    /// </summary>
    PasswordNotAllowed = 1132,
    /// <summary>
    /// The given password does not match
    /// </summary>
    PasswordNoMatch = 1133,
    ///<summary>Information returned by an UPDATE statement.</summary>
    ///<remarks>ER_UPDATE_INFO</remarks>
    UpdateInfo = 1134,
    ///<summary>A new thread couldn't be created.</summary>
    ///<remarks>ER_CANT_CREATE_THREAD</remarks>
    CannotCreateThread = 1135,
    ///<summary>The column count doesn't match the value count.</summary>
    ///<remarks>ER_WRONG_VALUE_COUNT_ON_ROW</remarks>
    WrongValueCountOnRow = 1136,
    ///<summary>The specified table can't be re-opened.</summary>
    ///<remarks>ER_CANT_REOPEN_TABLE</remarks>
    CannotReopenTable = 1137,
    ///<summary>The NULL value has been used incorrectly.</summary>
    ///<remarks>ER_INVALID_USE_OF_NULL</remarks>
    InvalidUseOfNull = 1138,
    ///<summary>The regular expression contains an error.</summary>
    ///<remarks>ER_REGEXP_ERROR</remarks>
    RegExpError = 1139,
    ///<summary>GROUP columns (MIN(), MAX(), COUNT(), ...) cannot be mixes with no GROUP columns if there is not GROUP BY clause.</summary>
    ///<remarks>ER_MIX_OF_GROUP_FUNC_AND_FIELDS</remarks>
    MixOfGroupFunctionAndFields = 1140,
    ///<summary></summary>
    ///<remarks>ER_NONEXISTING_GRANT</remarks>
    NonExistingGrant = 1141,
    ///<summary></summary>
    ///<remarks>ER_TABLEACCESS_DENIED_ERROR</remarks>
    TableAccessDenied = 1142,
    ///<summary></summary>
    ///<remarks>ER_COLUMNACCESS_DENIED_ERROR</remarks>
    ColumnAccessDenied = 1143,
    ///<summary></summary>
    ///<remarks>ER_ILLEGAL_GRANT_FOR_TABLE</remarks>
    IllegalGrantForTable = 1144,
    ///<summary></summary>
    ///<remarks>ER_GRANT_WRONG_HOST_OR_USER</remarks>
    GrantWrongHostOrUser = 1145,
    ///<summary></summary>
    ///<remarks>ER_NO_SUCH_TABLE</remarks>
    NoSuchTable = 1146,
    ///<summary></summary>
    ///<remarks>ER_NONEXISTING_TABLE_GRANT</remarks>
    NonExistingTableGrant = 1147,
    ///<summary></summary>
    ///<remarks>ER_NOT_ALLOWED_COMMAND</remarks>
    NotAllowedCommand = 1148,
    ///<summary></summary>
    ///<remarks>ER_SYNTAX_ERROR</remarks>
    SyntaxError = 1149,
    ///<summary></summary>
    ///<remarks>ER_DELAYED_CANT_CHANGE_LOCK</remarks>
    DelayedCannotChangeLock = 1150,
    ///<summary></summary>
    ///<remarks>ER_TOO_MANY_DELAYED_THREADS</remarks>
    TooManyDelayedThreads = 1151,
    ///<summary></summary>
    ///<remarks>ER_ABORTING_CONNECTION</remarks>
    AbortingConnection = 1152,
    /// <summary>
    /// An attempt was made to send or receive a packet larger than
    /// max_allowed_packet_size
    /// </summary>
    PacketTooLarge = 1153,
    ///<summary></summary>
    ///<remarks>ER_NET_READ_ERROR_FROM_PIPE</remarks>
    NetReadErrorFromPipe = 1154,
    ///<summary></summary>
    ///<remarks>ER_NET_FCNTL_ERROR</remarks>
    NetFCntlError = 1155,
    ///<summary></summary>
    ///<remarks>ER_NET_PACKETS_OUT_OF_ORDER</remarks>
    NetPacketsOutOfOrder = 1156,
    ///<summary></summary>
    ///<remarks>ER_NET_UNCOMPRESS_ERROR</remarks>
    NetUncompressError = 1157,
    ///<summary></summary>
    ///<remarks>ER_NET_READ_ERROR</remarks>
    NetReadError = 1158,
    ///<summary></summary>
    ///<remarks>ER_NET_READ_INTERRUPTED</remarks>
    NetReadInterrupted = 1159,
    ///<summary></summary>
    ///<remarks>ER_NET_ERROR_ON_WRITE</remarks>
    NetErrorOnWrite = 1160,
    ///<summary></summary>
    ///<remarks>ER_NET_WRITE_INTERRUPTED</remarks>
    NetWriteInterrupted = 1161,
    ///<summary></summary>
    ///<remarks>ER_TOO_LONG_STRING</remarks>
    TooLongString = 1162,
    ///<summary></summary>
    ///<remarks>ER_TABLE_CANT_HANDLE_BLOB</remarks>
    TableCannotHandleBlob = 1163,
    ///<summary></summary>
    ///<remarks>ER_TABLE_CANT_HANDLE_AUTO_INCREMENT</remarks>
    TableCannotHandleAutoIncrement = 1164,
    ///<summary></summary>
    ///<remarks>ER_DELAYED_INSERT_TABLE_LOCKED</remarks>
    DelayedInsertTableLocked = 1165,
    ///<summary></summary>
    ///<remarks>ER_WRONG_COLUMN_NAME</remarks>
    WrongColumnName = 1166,
    ///<summary></summary>
    ///<remarks>ER_WRONG_KEY_COLUMN</remarks>
    WrongKeyColumn = 1167,
    ///<summary></summary>
    ///<remarks>ER_WRONG_MRG_TABLE</remarks>
    WrongMergeTable = 1168,
    ///<summary></summary>
    ///<remarks>ER_DUP_UNIQUE</remarks>
    DuplicateUnique = 1169,
    ///<summary></summary>
    ///<remarks>ER_BLOB_KEY_WITHOUT_LENGTH</remarks>
    BlobKeyWithoutLength = 1170,
    ///<summary></summary>
    ///<remarks>ER_PRIMARY_CANT_HAVE_NULL</remarks>
    PrimaryCannotHaveNull = 1171,
    ///<summary></summary>
    ///<remarks>ER_TOO_MANY_ROWS</remarks>
    TooManyRows = 1172,
    ///<summary></summary>
    ///<remarks>ER_REQUIRES_PRIMARY_KEY</remarks>
    RequiresPrimaryKey = 1173,
    ///<summary></summary>
    ///<remarks>ER_NO_RAID_COMPILED</remarks>
    NoRAIDCompiled = 1174,
    ///<summary></summary>
    ///<remarks>ER_UPDATE_WITHOUT_KEY_IN_SAFE_MODE</remarks>
    UpdateWithoutKeysInSafeMode = 1175,
    ///<summary></summary>
    ///<remarks>ER_KEY_DOES_NOT_EXITS</remarks>
    KeyDoesNotExist = 1176,
    ///<summary></summary>
    ///<remarks>ER_CHECK_NO_SUCH_TABLE</remarks>
    CheckNoSuchTable = 1177,
    ///<summary></summary>
    ///<remarks>ER_CHECK_NOT_IMPLEMENTED</remarks>
    CheckNotImplemented = 1178,
    ///<summary></summary>
    ///<remarks>ER_CANT_DO_THIS_DURING_AN_TRANSACTION</remarks>
    CannotDoThisDuringATransaction = 1179,
    ///<summary></summary>
    ///<remarks>ER_ERROR_DURING_COMMIT</remarks>
    ErrorDuringCommit = 1180,
    ///<summary></summary>
    ///<remarks>ER_ERROR_DURING_ROLLBACK</remarks>
    ErrorDuringRollback = 1181,
    ///<summary></summary>
    ///<remarks>ER_ERROR_DURING_FLUSH_LOGS</remarks>
    ErrorDuringFlushLogs = 1182,
    ///<summary></summary>
    ///<remarks>ER_ERROR_DURING_CHECKPOINT</remarks>
    ErrorDuringCheckpoint = 1183,
    ///<summary></summary>
    ///<remarks>ER_NEW_ABORTING_CONNECTION</remarks>
    NewAbortingConnection = 1184,
    ///<summary></summary>
    ///<remarks>ER_DUMP_NOT_IMPLEMENTED</remarks>
    DumpNotImplemented = 1185,
    ///<summary></summary>
    ///<remarks>ER_FLUSH_MASTER_BINLOG_CLOSED</remarks>
    FlushMasterBinLogClosed = 1186,
    ///<summary></summary>
    ///<remarks>ER_INDEX_REBUILD</remarks>
    IndexRebuild = 1187,
    ///<summary></summary>
    ///<remarks>ER_MASTER</remarks>
    MasterError = 1188,
    ///<summary></summary>
    ///<remarks>ER_MASTER_NET_READ</remarks>
    MasterNetRead = 1189,
    ///<summary></summary>
    ///<remarks>ER_MASTER_NET_WRITE</remarks>
    MasterNetWrite = 1190,
    ///<summary></summary>
    ///<remarks>ER_FT_MATCHING_KEY_NOT_FOUND</remarks>
    FullTextMatchingKeyNotFound = 1191,
    ///<summary></summary>
    ///<remarks>ER_LOCK_OR_ACTIVE_TRANSACTION</remarks>
    LockOrActiveTransaction = 1192,
    ///<summary></summary>
    ///<remarks>ER_UNKNOWN_SYSTEM_VARIABLE</remarks>
    UnknownSystemVariable = 1193,
    ///<summary></summary>
    ///<remarks>ER_CRASHED_ON_USAGE</remarks>
    CrashedOnUsage = 1194,
    ///<summary></summary>
    ///<remarks>ER_CRASHED_ON_REPAIR</remarks>
    CrashedOnRepair = 1195,
    ///<summary></summary>
    ///<remarks>ER_WARNING_NOT_COMPLETE_ROLLBACK</remarks>
    WarningNotCompleteRollback = 1196,
    ///<summary></summary>
    ///<remarks>ER_TRANS_CACHE_FULL</remarks>
    TransactionCacheFull = 1197,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_MUST_STOP</remarks>
    SlaveMustStop = 1198,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_NOT_RUNNING</remarks>
    SlaveNotRunning = 1199,
    ///<summary></summary>
    ///<remarks>ER_BAD_SLAVE</remarks>
    BadSlave = 1200,
    ///<summary></summary>
    ///<remarks>ER_MASTER_INFO</remarks>
    MasterInfo = 1201,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_THREAD</remarks>
    SlaveThread = 1202,
    ///<summary></summary>
    ///<remarks>ER_TOO_MANY_USER_CONNECTIONS</remarks>
    TooManyUserConnections = 1203,
    ///<summary></summary>
    ///<remarks>ER_SET_CONSTANTS_ONLY</remarks>
    SetConstantsOnly = 1204,
    ///<summary></summary>
    ///<remarks>ER_LOCK_WAIT_TIMEOUT</remarks>
    LockWaitTimeout = 1205,
    ///<summary></summary>
    ///<remarks>ER_LOCK_TABLE_FULL</remarks>
    LockTableFull = 1206,
    ///<summary></summary>
    ///<remarks>ER_READ_ONLY_TRANSACTION</remarks>
    ReadOnlyTransaction = 1207,
    ///<summary></summary>
    ///<remarks>ER_DROP_DB_WITH_READ_LOCK</remarks>
    DropDatabaseWithReadLock = 1208,
    ///<summary></summary>
    ///<remarks>ER_CREATE_DB_WITH_READ_LOCK</remarks>
    CreateDatabaseWithReadLock = 1209,
    ///<summary></summary>
    ///<remarks>ER_WRONG_ARGUMENTS</remarks>
    WrongArguments = 1210,
    ///<summary></summary>
    ///<remarks>ER_NO_PERMISSION_TO_CREATE_USER</remarks>
    NoPermissionToCreateUser = 1211,
    ///<summary></summary>
    ///<remarks>ER_UNION_TABLES_IN_DIFFERENT_DIR</remarks>
    UnionTablesInDifferentDirectory = 1212,
    ///<summary></summary>
    ///<remarks>ER_LOCK_DEADLOCK</remarks>
    LockDeadlock = 1213,
    ///<summary></summary>
    ///<remarks>ER_TABLE_CANT_HANDLE_FT</remarks>
    TableCannotHandleFullText = 1214,
    ///<summary></summary>
    ///<remarks>ER_CANNOT_ADD_FOREIGN</remarks>
    CannotAddForeignConstraint = 1215,
    ///<summary></summary>
    ///<remarks>ER_NO_REFERENCED_ROW</remarks>
    NoReferencedRow = 1216,
    ///<summary></summary>
    ///<remarks>ER_ROW_IS_REFERENCED</remarks>
    RowIsReferenced = 1217,
    ///<summary></summary>
    ///<remarks>ER_CONNECT_TO_MASTER</remarks>
    ConnectToMaster = 1218,
    ///<summary></summary>
    ///<remarks>ER_QUERY_ON_MASTER</remarks>
    QueryOnMaster = 1219,
    ///<summary></summary>
    ///<remarks>ER_ERROR_WHEN_EXECUTING_COMMAND</remarks>
    ErrorWhenExecutingCommand = 1220,
    ///<summary></summary>
    ///<remarks>ER_WRONG_USAGE</remarks>
    WrongUsage = 1221,
    ///<summary></summary>
    ///<remarks>ER_WRONG_NUMBER_OF_COLUMNS_IN_SELECT</remarks>
    WrongNumberOfColumnsInSelect = 1222,
    ///<summary></summary>
    ///<remarks>ER_CANT_UPDATE_WITH_READLOCK</remarks>
    CannotUpdateWithReadLock = 1223,
    ///<summary></summary>
    ///<remarks>ER_MIXING_NOT_ALLOWED</remarks>
    MixingNotAllowed = 1224,
    ///<summary></summary>
    ///<remarks>ER_DUP_ARGUMENT</remarks>
    DuplicateArgument = 1225,
    ///<summary></summary>
    ///<remarks>ER_USER_LIMIT_REACHED</remarks>
    UserLimitReached = 1226,
    ///<summary></summary>
    ///<remarks>ER_SPECIFIC_ACCESS_DENIED_ERROR</remarks>
    SpecifiedAccessDeniedError = 1227,
    ///<summary></summary>
    ///<remarks>ER_LOCAL_VARIABLE</remarks>
    LocalVariableError = 1228,
    ///<summary></summary>
    ///<remarks>ER_GLOBAL_VARIABLE</remarks>
    GlobalVariableError = 1229,
    ///<summary></summary>
    ///<remarks>ER_NO_DEFAULT</remarks>
    NotDefaultError = 1230,
    ///<summary></summary>
    ///<remarks>ER_WRONG_VALUE_FOR_VAR</remarks>
    WrongValueForVariable = 1231,
    ///<summary></summary>
    ///<remarks>ER_WRONG_TYPE_FOR_VAR</remarks>
    WrongTypeForVariable = 1232,
    ///<summary></summary>
    ///<remarks>ER_VAR_CANT_BE_READ</remarks>
    VariableCannotBeRead = 1233,
    ///<summary></summary>
    ///<remarks>ER_CANT_USE_OPTION_HERE</remarks>
    CannotUseOptionHere = 1234,
    ///<summary></summary>
    ///<remarks>ER_NOT_SUPPORTED_YET</remarks>
    NotSupportedYet = 1235,
    ///<summary></summary>
    ///<remarks>ER_MASTER_FATAL_ERROR_READING_BINLOG</remarks>
    MasterFatalErrorReadingBinLog = 1236,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_IGNORED_TABLE</remarks>
    SlaveIgnoredTable = 1237,
    ///<summary></summary>
    ///<remarks>ER_INCORRECT_GLOBAL_LOCAL_VAR</remarks>
    IncorrectGlobalLocalVariable = 1238,
    ///<summary></summary>
    ///<remarks>ER_WRONG_FK_DEF</remarks>
    WrongForeignKeyDefinition = 1239,
    ///<summary></summary>
    ///<remarks>ER_KEY_REF_DO_NOT_MATCH_TABLE_REF</remarks>
    KeyReferenceDoesNotMatchTableReference = 1240,
    ///<summary></summary>
    ///<remarks>ER_OPERAND_COLUMNS</remarks>
    OpearnColumnsError = 1241,
    ///<summary></summary>
    ///<remarks>ER_SUBQUERY_NO_1_ROW</remarks>
    SubQueryNoOneRow = 1242,
    ///<summary></summary>
    ///<remarks>ER_UNKNOWN_STMT_HANDLER</remarks>
    UnknownStatementHandler = 1243,
    ///<summary></summary>
    ///<remarks>ER_CORRUPT_HELP_DB</remarks>
    CorruptHelpDatabase = 1244,
    ///<summary></summary>
    ///<remarks>ER_CYCLIC_REFERENCE</remarks>
    CyclicReference = 1245,
    ///<summary></summary>
    ///<remarks>ER_AUTO_CONVERT</remarks>
    AutoConvert = 1246,
    ///<summary></summary>
    ///<remarks>ER_ILLEGAL_REFERENCE</remarks>
    IllegalReference = 1247,
    ///<summary></summary>
    ///<remarks>ER_DERIVED_MUST_HAVE_ALIAS</remarks>
    DerivedMustHaveAlias = 1248,
    ///<summary></summary>
    ///<remarks>ER_SELECT_REDUCED</remarks>
    SelectReduced = 1249,
    ///<summary></summary>
    ///<remarks>ER_TABLENAME_NOT_ALLOWED_HERE</remarks>
    TableNameNotAllowedHere = 1250,
    ///<summary></summary>
    ///<remarks>ER_NOT_SUPPORTED_AUTH_MODE</remarks>
    NotSupportedAuthMode = 1251,
    ///<summary></summary>
    ///<remarks>ER_SPATIAL_CANT_HAVE_NULL</remarks>
    SpatialCannotHaveNull = 1252,
    ///<summary></summary>
    ///<remarks>ER_COLLATION_CHARSET_MISMATCH</remarks>
    CollationCharsetMismatch = 1253,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_WAS_RUNNING</remarks>
    SlaveWasRunning = 1254,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_WAS_NOT_RUNNING</remarks>
    SlaveWasNotRunning = 1255,
    ///<summary></summary>
    ///<remarks>ER_TOO_BIG_FOR_UNCOMPRESS</remarks>
    TooBigForUncompress = 1256,
    ///<summary></summary>
    ///<remarks>ER_ZLIB_Z_MEM_ERROR</remarks>
    ZipLibMemoryError = 1257,
    ///<summary></summary>
    ///<remarks>ER_ZLIB_Z_BUF_ERROR</remarks>
    ZipLibBufferError = 1258,
    ///<summary></summary>
    ///<remarks>ER_ZLIB_Z_DATA_ERROR</remarks>
    ZipLibDataError = 1259,
    ///<summary></summary>
    ///<remarks>ER_CUT_VALUE_GROUP_CONCAT</remarks>
    CutValueGroupConcat = 1260,
    ///<summary></summary>
    ///<remarks>ER_WARN_TOO_FEW_RECORDS</remarks>
    WarningTooFewRecords = 1261,
    ///<summary></summary>
    ///<remarks>ER_WARN_TOO_MANY_RECORDS</remarks>
    WarningTooManyRecords = 1262,
    ///<summary></summary>
    ///<remarks>ER_WARN_NULL_TO_NOTNULL</remarks>
    WarningNullToNotNull = 1263,
    ///<summary></summary>
    ///<remarks>ER_WARN_DATA_OUT_OF_RANGE</remarks>
    WarningDataOutOfRange = 1264,
    ///<summary></summary>
    ///<remarks>WARN_DATA_TRUNCATED</remarks>
    WaningDataTruncated = 1265,
    ///<summary></summary>
    ///<remarks>ER_WARN_USING_OTHER_HANDLER</remarks>
    WaningUsingOtherHandler = 1266,
    ///<summary></summary>
    ///<remarks>ER_CANT_AGGREGATE_2COLLATIONS</remarks>
    CannotAggregateTwoCollations = 1267,
    ///<summary></summary>
    ///<remarks>ER_DROP_USER</remarks>
    DropUserError = 1268,
    ///<summary></summary>
    ///<remarks>ER_REVOKE_GRANTS</remarks>
    RevokeGrantsError = 1269,
    ///<summary></summary>
    ///<remarks>ER_CANT_AGGREGATE_3COLLATIONS</remarks>
    CannotAggregateThreeCollations = 1270,
    ///<summary></summary>
    ///<remarks>ER_CANT_AGGREGATE_NCOLLATIONS</remarks>
    CannotAggregateNCollations = 1271,
    ///<summary></summary>
    ///<remarks>ER_VARIABLE_IS_NOT_STRUCT</remarks>
    VariableIsNotStructure = 1272,
    ///<summary></summary>
    ///<remarks>ER_UNKNOWN_COLLATION</remarks>
    UnknownCollation = 1273,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_IGNORED_SSL_PARAMS</remarks>
    SlaveIgnoreSSLParameters = 1274,
    ///<summary></summary>
    ///<remarks>ER_SERVER_IS_IN_SECURE_AUTH_MODE</remarks>
    ServerIsInSecureAuthMode = 1275,
    ///<summary></summary>
    ///<remarks>ER_WARN_FIELD_RESOLVED</remarks>
    WaningFieldResolved = 1276,
    ///<summary></summary>
    ///<remarks>ER_BAD_SLAVE_UNTIL_COND</remarks>
    BadSlaveUntilCondition = 1277,
    ///<summary></summary>
    ///<remarks>ER_MISSING_SKIP_SLAVE</remarks>
    MissingSkipSlave = 1278,
    ///<summary></summary>
    ///<remarks>ER_UNTIL_COND_IGNORED</remarks>
    ErrorUntilConditionIgnored = 1279,
    ///<summary></summary>
    ///<remarks>ER_WRONG_NAME_FOR_INDEX</remarks>
    WrongNameForIndex = 1280,
    ///<summary></summary>
    ///<remarks>ER_WRONG_NAME_FOR_CATALOG</remarks>
    WrongNameForCatalog = 1281,
    ///<summary></summary>
    ///<remarks>ER_WARN_QC_RESIZE</remarks>
    WarningQueryCacheResize = 1282,
    ///<summary></summary>
    ///<remarks>ER_BAD_FT_COLUMN</remarks>
    BadFullTextColumn = 1283,
    ///<summary></summary>
    ///<remarks>ER_UNKNOWN_KEY_CACHE</remarks>
    UnknownKeyCache = 1284,
    ///<summary></summary>
    ///<remarks>ER_WARN_HOSTNAME_WONT_WORK</remarks>
    WarningHostnameWillNotWork = 1285,
    ///<summary></summary>
    ///<remarks>ER_UNKNOWN_STORAGE_ENGINE</remarks>
    UnknownStorageEngine = 1286,
    ///<summary></summary>
    ///<remarks>ER_WARN_DEPRECATED_SYNTAX</remarks>
    WaningDeprecatedSyntax = 1287,
    ///<summary></summary>
    ///<remarks>ER_NON_UPDATABLE_TABLE</remarks>
    NonUpdateableTable = 1288,
    ///<summary></summary>
    ///<remarks>ER_FEATURE_DISABLED</remarks>
    FeatureDisabled = 1289,
    ///<summary></summary>
    ///<remarks>ER_OPTION_PREVENTS_STATEMENT</remarks>
    OptionPreventsStatement = 1290,
    ///<summary></summary>
    ///<remarks>ER_DUPLICATED_VALUE_IN_TYPE</remarks>
    DuplicatedValueInType = 1291,
    ///<summary></summary>
    ///<remarks>ER_TRUNCATED_WRONG_VALUE</remarks>
    TruncatedWrongValue = 1292,
    ///<summary></summary>
    ///<remarks>ER_TOO_MUCH_AUTO_TIMESTAMP_COLS</remarks>
    TooMuchAutoTimestampColumns = 1293,
    ///<summary></summary>
    ///<remarks>ER_INVALID_ON_UPDATE</remarks>
    InvalidOnUpdate = 1294,
    ///<summary></summary>
    ///<remarks>ER_UNSUPPORTED_PS</remarks>
    UnsupportedPreparedStatement = 1295,
    ///<summary></summary>
    ///<remarks>ER_GET_ERRMSG</remarks>
    GetErroMessage = 1296,
    ///<summary></summary>
    ///<remarks>ER_GET_TEMPORARY_ERRMSG</remarks>
    GetTemporaryErrorMessage = 1297,
    ///<summary></summary>
    ///<remarks>ER_UNKNOWN_TIME_ZONE</remarks>
    UnknownTimeZone = 1298,
    ///<summary></summary>
    ///<remarks>ER_WARN_INVALID_TIMESTAMP</remarks>
    WarningInvalidTimestamp = 1299,
    ///<summary></summary>
    ///<remarks>ER_INVALID_CHARACTER_STRING</remarks>
    InvalidCharacterString = 1300,
    ///<summary></summary>
    ///<remarks>ER_WARN_ALLOWED_PACKET_OVERFLOWED</remarks>
    WarningAllowedPacketOverflowed = 1301,
    ///<summary></summary>
    ///<remarks>ER_CONFLICTING_DECLARATIONS</remarks>
    ConflictingDeclarations = 1302,
    ///<summary></summary>
    ///<remarks>ER_SP_NO_RECURSIVE_CREATE</remarks>
    StoredProcedureNoRecursiveCreate = 1303,
    ///<summary></summary>
    ///<remarks>ER_SP_ALREADY_EXISTS</remarks>
    StoredProcedureAlreadyExists = 1304,
    ///<summary></summary>
    ///<remarks>ER_SP_DOES_NOT_EXIST</remarks>
    StoredProcedureDoesNotExist = 1305,
    ///<summary></summary>
    ///<remarks>ER_SP_DROP_FAILED</remarks>
    StoredProcedureDropFailed = 1306,
    ///<summary></summary>
    ///<remarks>ER_SP_STORE_FAILED</remarks>
    StoredProcedureStoreFailed = 1307,
    ///<summary></summary>
    ///<remarks>ER_SP_LILABEL_MISMATCH</remarks>
    StoredProcedureLiLabelMismatch = 1308,
    ///<summary></summary>
    ///<remarks>ER_SP_LABEL_REDEFINE</remarks>
    StoredProcedureLabelRedefine = 1309,
    ///<summary></summary>
    ///<remarks>ER_SP_LABEL_MISMATCH</remarks>
    StoredProcedureLabelMismatch = 1310,
    ///<summary></summary>
    ///<remarks>ER_SP_UNINIT_VAR</remarks>
    StoredProcedureUninitializedVariable = 1311,
    ///<summary></summary>
    ///<remarks>ER_SP_BADSELECT</remarks>
    StoredProcedureBadSelect = 1312,
    ///<summary></summary>
    ///<remarks>ER_SP_BADRETURN</remarks>
    StoredProcedureBadReturn = 1313,
    ///<summary></summary>
    ///<remarks>ER_SP_BADSTATEMENT</remarks>
    StoredProcedureBadStatement = 1314,
    ///<summary></summary>
    ///<remarks>ER_UPDATE_LOG_DEPRECATED_IGNORED</remarks>
    UpdateLogDeprecatedIgnored = 1315,
    ///<summary></summary>
    ///<remarks>ER_UPDATE_LOG_DEPRECATED_TRANSLATED</remarks>
    UpdateLogDeprecatedTranslated = 1316,
    ///<summary></summary>
    ///<remarks>ER_QUERY_INTERRUPTED</remarks>
    QueryInterrupted = 1317,
    ///<summary></summary>
    ///<remarks>ER_SP_WRONG_NO_OF_ARGS</remarks>
    StoredProcedureNumberOfArguments = 1318,
    ///<summary></summary>
    ///<remarks>ER_SP_COND_MISMATCH</remarks>
    StoredProcedureConditionMismatch = 1319,
    ///<summary></summary>
    ///<remarks>ER_SP_NORETURN</remarks>
    StoredProcedureNoReturn = 1320,
    ///<summary></summary>
    ///<remarks>ER_SP_NORETURNEND</remarks>
    StoredProcedureNoReturnEnd = 1321,
    ///<summary></summary>
    ///<remarks>ER_SP_BAD_CURSOR_QUERY</remarks>
    StoredProcedureBadCursorQuery = 1322,
    ///<summary></summary>
    ///<remarks>ER_SP_BAD_CURSOR_SELECT</remarks>
    StoredProcedureBadCursorSelect = 1323,
    ///<summary></summary>
    ///<remarks>ER_SP_CURSOR_MISMATCH</remarks>
    StoredProcedureCursorMismatch = 1324,
    ///<summary></summary>
    ///<remarks>ER_SP_CURSOR_ALREADY_OPEN</remarks>
    StoredProcedureAlreadyOpen = 1325,
    ///<summary></summary>
    ///<remarks>ER_SP_CURSOR_NOT_OPEN</remarks>
    StoredProcedureCursorNotOpen = 1326,
    ///<summary></summary>
    ///<remarks>ER_SP_UNDECLARED_VAR</remarks>
    StoredProcedureUndeclaredVariabel = 1327,
    ///<summary></summary>
    ///<remarks>ER_SP_WRONG_NO_OF_FETCH_ARGS</remarks>
    StoredProcedureWrongNumberOfFetchArguments = 1328,
    ///<summary></summary>
    ///<remarks>ER_SP_FETCH_NO_DATA</remarks>
    StoredProcedureFetchNoData = 1329,
    ///<summary></summary>
    ///<remarks>ER_SP_DUP_PARAM</remarks>
    StoredProcedureDuplicateParameter = 1330,
    ///<summary></summary>
    ///<remarks>ER_SP_DUP_VAR</remarks>
    StoredProcedureDuplicateVariable = 1331,
    ///<summary></summary>
    ///<remarks>ER_SP_DUP_COND</remarks>
    StoredProcedureDuplicateCondition = 1332,
    ///<summary></summary>
    ///<remarks>ER_SP_DUP_CURS</remarks>
    StoredProcedureDuplicateCursor = 1333,
    ///<summary></summary>
    ///<remarks>ER_SP_CANT_ALTER</remarks>
    StoredProcedureCannotAlter = 1334,
    ///<summary></summary>
    ///<remarks>ER_SP_SUBSELECT_NYI</remarks>
    StoredProcedureSubSelectNYI = 1335,
    ///<summary></summary>
    ///<remarks>ER_STMT_NOT_ALLOWED_IN_SF_OR_TRG</remarks>
    StatementNotAllowedInStoredFunctionOrTrigger = 1336,
    ///<summary></summary>
    ///<remarks>ER_SP_VARCOND_AFTER_CURSHNDLR</remarks>
    StoredProcedureVariableConditionAfterCursorHandler = 1337,
    ///<summary></summary>
    ///<remarks>ER_SP_CURSOR_AFTER_HANDLER</remarks>
    StoredProcedureCursorAfterHandler = 1338,
    ///<summary></summary>
    ///<remarks>ER_SP_CASE_NOT_FOUND</remarks>
    StoredProcedureCaseNotFound = 1339,
    ///<summary></summary>
    ///<remarks>ER_FPARSER_TOO_BIG_FILE</remarks>
    FileParserTooBigFile = 1340,
    ///<summary></summary>
    ///<remarks>ER_FPARSER_BAD_HEADER</remarks>
    FileParserBadHeader = 1341,
    ///<summary></summary>
    ///<remarks>ER_FPARSER_EOF_IN_COMMENT</remarks>
    FileParserEOFInComment = 1342,
    ///<summary></summary>
    ///<remarks>ER_FPARSER_ERROR_IN_PARAMETER</remarks>
    FileParserErrorInParameter = 1343,
    ///<summary></summary>
    ///<remarks>ER_FPARSER_EOF_IN_UNKNOWN_PARAMETER</remarks>
    FileParserEOFInUnknownParameter = 1344,
    ///<summary></summary>
    ///<remarks>ER_VIEW_NO_EXPLAIN</remarks>
    ViewNoExplain = 1345,
    ///<summary></summary>
    ///<remarks>ER_FRM_UNKNOWN_TYPE</remarks>
    FrmUnknownType = 1346,
    ///<summary></summary>
    ///<remarks>ER_WRONG_OBJECT</remarks>
    WrongObject = 1347,
    ///<summary></summary>
    ///<remarks>ER_NONUPDATEABLE_COLUMN</remarks>
    NonUpdateableColumn = 1348,
    ///<summary></summary>
    ///<remarks>ER_VIEW_SELECT_DERIVED</remarks>
    ViewSelectDerived = 1349,
    ///<summary></summary>
    ///<remarks>ER_VIEW_SELECT_CLAUSE</remarks>
    ViewSelectClause = 1350,
    ///<summary></summary>
    ///<remarks>ER_VIEW_SELECT_VARIABLE</remarks>
    ViewSelectVariable = 1351,
    ///<summary></summary>
    ///<remarks>ER_VIEW_SELECT_TMPTABLE</remarks>
    ViewSelectTempTable = 1352,
    ///<summary></summary>
    ///<remarks>ER_VIEW_WRONG_LIST</remarks>
    ViewWrongList = 1353,
    ///<summary></summary>
    ///<remarks>ER_WARN_VIEW_MERGE</remarks>
    WarningViewMerge = 1354,
    ///<summary></summary>
    ///<remarks>ER_WARN_VIEW_WITHOUT_KEY</remarks>
    WarningViewWithoutKey = 1355,
    ///<summary></summary>
    ///<remarks>ER_VIEW_INVALID</remarks>
    ViewInvalid = 1356,
    ///<summary></summary>
    ///<remarks>ER_SP_NO_DROP_SP</remarks>
    StoredProcedureNoDropStoredProcedure = 1357,
    ///<summary></summary>
    ///<remarks>ER_SP_GOTO_IN_HNDLR</remarks>
    StoredProcedureGotoInHandler = 1358,
    ///<summary></summary>
    ///<remarks>ER_TRG_ALREADY_EXISTS</remarks>
    TriggerAlreadyExists = 1359,
    ///<summary></summary>
    ///<remarks>ER_TRG_DOES_NOT_EXIST</remarks>
    TriggerDoesNotExist = 1360,
    ///<summary></summary>
    ///<remarks>ER_TRG_ON_VIEW_OR_TEMP_TABLE</remarks>
    TriggerOnViewOrTempTable = 1361,
    ///<summary></summary>
    ///<remarks>ER_TRG_CANT_CHANGE_ROW</remarks>
    TriggerCannotChangeRow = 1362,
    ///<summary></summary>
    ///<remarks>ER_TRG_NO_SUCH_ROW_IN_TRG</remarks>
    TriggerNoSuchRowInTrigger = 1363,
    ///<summary></summary>
    ///<remarks>ER_NO_DEFAULT_FOR_FIELD</remarks>
    NoDefaultForField = 1364,
    ///<summary></summary>
    ///<remarks>ER_DIVISION_BY_ZERO</remarks>
    DivisionByZero = 1365,
    ///<summary></summary>
    ///<remarks>ER_TRUNCATED_WRONG_VALUE_FOR_FIELD</remarks>
    TruncatedWrongValueForField = 1366,
    ///<summary></summary>
    ///<remarks>ER_ILLEGAL_VALUE_FOR_TYPE</remarks>
    IllegalValueForType = 1367,
    ///<summary></summary>
    ///<remarks>ER_VIEW_NONUPD_CHECK</remarks>
    ViewNonUpdatableCheck = 1368,
    ///<summary></summary>
    ///<remarks>ER_VIEW_CHECK_FAILED</remarks>
    ViewCheckFailed = 1369,
    ///<summary></summary>
    ///<remarks>ER_PROCACCESS_DENIED_ERROR</remarks>
    PrecedureAccessDenied = 1370,
    ///<summary></summary>
    ///<remarks>ER_RELAY_LOG_FAIL</remarks>
    RelayLogFail = 1371,
    ///<summary></summary>
    ///<remarks>ER_PASSWD_LENGTH</remarks>
    PasswordLength = 1372,
    ///<summary></summary>
    ///<remarks>ER_UNKNOWN_TARGET_BINLOG</remarks>
    UnknownTargetBinLog = 1373,
    ///<summary></summary>
    ///<remarks>ER_IO_ERR_LOG_INDEX_READ</remarks>
    IOErrorLogIndexRead = 1374,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_PURGE_PROHIBITED</remarks>
    BinLogPurgeProhibited = 1375,
    ///<summary></summary>
    ///<remarks>ER_FSEEK_FAIL</remarks>
    FSeekFail = 1376,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_PURGE_FATAL_ERR</remarks>
    BinLogPurgeFatalError = 1377,
    ///<summary></summary>
    ///<remarks>ER_LOG_IN_USE</remarks>
    LogInUse = 1378,
    ///<summary></summary>
    ///<remarks>ER_LOG_PURGE_UNKNOWN_ERR</remarks>
    LogPurgeUnknownError = 1379,
    ///<summary></summary>
    ///<remarks>ER_RELAY_LOG_INIT</remarks>
    RelayLogInit = 1380,
    ///<summary></summary>
    ///<remarks>ER_NO_BINARY_LOGGING</remarks>
    NoBinaryLogging = 1381,
    ///<summary></summary>
    ///<remarks>ER_RESERVED_SYNTAX</remarks>
    ReservedSyntax = 1382,
    ///<summary></summary>
    ///<remarks>ER_WSAS_FAILED</remarks>
    WSAStartupFailed = 1383,
    ///<summary></summary>
    ///<remarks>ER_DIFF_GROUPS_PROC</remarks>
    DifferentGroupsProcedure = 1384,
    ///<summary></summary>
    ///<remarks>ER_NO_GROUP_FOR_PROC</remarks>
    NoGroupForProcedure = 1385,
    ///<summary></summary>
    ///<remarks>ER_ORDER_WITH_PROC</remarks>
    OrderWithProcedure = 1386,
    ///<summary></summary>
    ///<remarks>ER_LOGGING_PROHIBIT_CHANGING_OF</remarks>
    LoggingProhibitsChangingOf = 1387,
    ///<summary></summary>
    ///<remarks>ER_NO_FILE_MAPPING</remarks>
    NoFileMapping = 1388,
    ///<summary></summary>
    ///<remarks>ER_WRONG_MAGIC</remarks>
    WrongMagic = 1389,
    ///<summary></summary>
    ///<remarks>ER_PS_MANY_PARAM</remarks>
    PreparedStatementManyParameters = 1390,
    ///<summary></summary>
    ///<remarks>ER_KEY_PART_0</remarks>
    KeyPartZero = 1391,
    ///<summary></summary>
    ///<remarks>ER_VIEW_CHECKSUM</remarks>
    ViewChecksum = 1392,
    ///<summary></summary>
    ///<remarks>ER_VIEW_MULTIUPDATE</remarks>
    ViewMultiUpdate = 1393,
    ///<summary></summary>
    ///<remarks>ER_VIEW_NO_INSERT_FIELD_LIST</remarks>
    ViewNoInsertFieldList = 1394,
    ///<summary></summary>
    ///<remarks>ER_VIEW_DELETE_MERGE_VIEW</remarks>
    ViewDeleteMergeView = 1395,
    ///<summary></summary>
    ///<remarks>ER_CANNOT_USER</remarks>
    CannotUser = 1396,
    ///<summary></summary>
    ///<remarks>ER_XAER_NOTA</remarks>
    XAERNotA = 1397,
    ///<summary></summary>
    ///<remarks>ER_XAER_INVAL</remarks>
    XAERInvalid = 1398,
    ///<summary></summary>
    ///<remarks>ER_XAER_RMFAIL</remarks>
    XAERRemoveFail = 1399,
    ///<summary></summary>
    ///<remarks>ER_XAER_OUTSIDE</remarks>
    XAEROutside = 1400,
    ///<summary></summary>
    ///<remarks>ER_XAER_RMERR</remarks>
    XAERRemoveError = 1401,
    ///<summary></summary>
    ///<remarks>ER_XA_RBROLLBACK</remarks>
    XARBRollback = 1402,
    ///<summary></summary>
    ///<remarks>ER_NONEXISTING_PROC_GRANT</remarks>
    NonExistingProcedureGrant = 1403,
    ///<summary></summary>
    ///<remarks>ER_PROC_AUTO_GRANT_FAIL</remarks>
    ProcedureAutoGrantFail = 1404,
    ///<summary></summary>
    ///<remarks>ER_PROC_AUTO_REVOKE_FAIL</remarks>
    ProcedureAutoRevokeFail = 1405,
    ///<summary></summary>
    ///<remarks>ER_DATA_TOO_LONG</remarks>
    DataTooLong = 1406,
    ///<summary></summary>
    ///<remarks>ER_SP_BAD_SQLSTATE</remarks>
    StoredProcedureSQLState = 1407,
    ///<summary></summary>
    ///<remarks>ER_STARTUP</remarks>
    StartupError = 1408,
    ///<summary></summary>
    ///<remarks>ER_LOAD_FROM_FIXED_SIZE_ROWS_TO_VAR</remarks>
    LoadFromFixedSizeRowsToVariable = 1409,
    ///<summary></summary>
    ///<remarks>ER_CANT_CREATE_USER_WITH_GRANT</remarks>
    CannotCreateUserWithGrant = 1410,
    ///<summary></summary>
    ///<remarks>ER_WRONG_VALUE_FOR_TYPE</remarks>
    WrongValueForType = 1411,
    ///<summary></summary>
    ///<remarks>ER_TABLE_DEF_CHANGED</remarks>
    TableDefinitionChanged = 1412,
    ///<summary></summary>
    ///<remarks>ER_SP_DUP_HANDLER</remarks>
    StoredProcedureDuplicateHandler = 1413,
    ///<summary></summary>
    ///<remarks>ER_SP_NOT_VAR_ARG</remarks>
    StoredProcedureNotVariableArgument = 1414,
    ///<summary></summary>
    ///<remarks>ER_SP_NO_RETSET</remarks>
    StoredProcedureNoReturnSet = 1415,
    ///<summary></summary>
    ///<remarks>ER_CANT_CREATE_GEOMETRY_OBJECT</remarks>
    CannotCreateGeometryObject = 1416,
    ///<summary></summary>
    ///<remarks>ER_FAILED_ROUTINE_BREAK_BINLOG</remarks>
    FailedRoutineBreaksBinLog = 1417,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_UNSAFE_ROUTINE</remarks>
    BinLogUnsafeRoutine = 1418,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_CREATE_ROUTINE_NEED_SUPER</remarks>
    BinLogCreateRoutineNeedSuper = 1419,
    ///<summary></summary>
    ///<remarks>ER_EXEC_STMT_WITH_OPEN_CURSOR</remarks>
    ExecuteStatementWithOpenCursor = 1420,
    ///<summary></summary>
    ///<remarks>ER_STMT_HAS_NO_OPEN_CURSOR</remarks>
    StatementHasNoOpenCursor = 1421,
    ///<summary></summary>
    ///<remarks>ER_COMMIT_NOT_ALLOWED_IN_SF_OR_TRG</remarks>
    CommitNotAllowedIfStoredFunctionOrTrigger = 1422,
    ///<summary></summary>
    ///<remarks>ER_NO_DEFAULT_FOR_VIEW_FIELD</remarks>
    NoDefaultForViewField = 1423,
    ///<summary></summary>
    ///<remarks>ER_SP_NO_RECURSION</remarks>
    StoredProcedureNoRecursion = 1424,
    ///<summary></summary>
    ///<remarks>ER_TOO_BIG_SCALE</remarks>
    TooBigScale = 1425,
    ///<summary></summary>
    ///<remarks>ER_TOO_BIG_PRECISION</remarks>
    TooBigPrecision = 1426,
    ///<summary></summary>
    ///<remarks>ER_M_BIGGER_THAN_D</remarks>
    MBiggerThanD = 1427,
    ///<summary></summary>
    ///<remarks>ER_WRONG_LOCK_OF_SYSTEM_TABLE</remarks>
    WrongLockOfSystemTable = 1428,
    ///<summary></summary>
    ///<remarks>ER_CONNECT_TO_FOREIGN_DATA_SOURCE</remarks>
    ConnectToForeignDataSource = 1429,
    ///<summary></summary>
    ///<remarks>ER_QUERY_ON_FOREIGN_DATA_SOURCE</remarks>
    QueryOnForeignDataSource = 1430,
    ///<summary></summary>
    ///<remarks>ER_FOREIGN_DATA_SOURCE_DOESNT_EXIST</remarks>
    ForeignDataSourceDoesNotExist = 1431,
    ///<summary></summary>
    ///<remarks>ER_FOREIGN_DATA_STRING_INVALID_CANT_CREATE</remarks>
    ForeignDataStringInvalidCannotCreate = 1432,
    ///<summary></summary>
    ///<remarks>ER_FOREIGN_DATA_STRING_INVALID</remarks>
    ForeignDataStringInvalid = 1433,
    ///<summary></summary>
    ///<remarks>ER_CANT_CREATE_FEDERATED_TABLE</remarks>
    CannotCreateFederatedTable = 1434,
    ///<summary></summary>
    ///<remarks>ER_TRG_IN_WRONG_SCHEMA</remarks>
    TriggerInWrongSchema = 1435,
    ///<summary></summary>
    ///<remarks>ER_STACK_OVERRUN_NEED_MORE</remarks>
    StackOverrunNeedMore = 1436,
    ///<summary></summary>
    ///<remarks>ER_TOO_LONG_BODY</remarks>
    TooLongBody = 1437,
    ///<summary></summary>
    ///<remarks>ER_WARN_CANT_DROP_DEFAULT_KEYCACHE</remarks>
    WarningCannotDropDefaultKeyCache = 1438,
    ///<summary></summary>
    ///<remarks>ER_TOO_BIG_DISPLAYWIDTH</remarks>
    TooBigDisplayWidth = 1439,
    ///<summary></summary>
    ///<remarks>ER_XAER_DUPID</remarks>
    XAERDuplicateID = 1440,
    ///<summary></summary>
    ///<remarks>ER_DATETIME_FUNCTION_OVERFLOW</remarks>
    DateTimeFunctionOverflow = 1441,
    ///<summary></summary>
    ///<remarks>ER_CANT_UPDATE_USED_TABLE_IN_SF_OR_TRG</remarks>
    CannotUpdateUsedTableInStoredFunctionOrTrigger = 1442,
    ///<summary></summary>
    ///<remarks>ER_VIEW_PREVENT_UPDATE</remarks>
    ViewPreventUpdate = 1443,
    ///<summary></summary>
    ///<remarks>ER_PS_NO_RECURSION</remarks>
    PreparedStatementNoRecursion = 1444,
    ///<summary></summary>
    ///<remarks>ER_SP_CANT_SET_AUTOCOMMIT</remarks>
    StoredProcedureCannotSetAutoCommit = 1445,
    ///<summary></summary>
    ///<remarks>ER_MALFORMED_DEFINER</remarks>
    MalformedDefiner = 1446,
    ///<summary></summary>
    ///<remarks>ER_VIEW_FRM_NO_USER</remarks>
    ViewFrmNoUser = 1447,
    ///<summary></summary>
    ///<remarks>ER_VIEW_OTHER_USER</remarks>
    ViewOtherUser = 1448,
    ///<summary></summary>
    ///<remarks>ER_NO_SUCH_USER</remarks>
    NoSuchUser = 1449,
    ///<summary></summary>
    ///<remarks>ER_FORBID_SCHEMA_CHANGE</remarks>
    ForbidSchemaChange = 1450,
    ///<summary></summary>
    ///<remarks>ER_ROW_IS_REFERENCED_2</remarks>
    RowIsReferenced2 = 1451,
    ///<summary></summary>
    ///<remarks>ER_NO_REFERENCED_ROW_2</remarks>
    NoReferencedRow2 = 1452,
    ///<summary></summary>
    ///<remarks>ER_SP_BAD_VAR_SHADOW</remarks>
    StoredProcedureBadVariableShadow = 1453,
    ///<summary></summary>
    ///<remarks>ER_TRG_NO_DEFINER</remarks>
    TriggerNoDefiner = 1454,
    ///<summary></summary>
    ///<remarks>ER_OLD_FILE_FORMAT</remarks>
    OldFileFormat = 1455,
    ///<summary></summary>
    ///<remarks>ER_SP_RECURSION_LIMIT</remarks>
    StoredProcedureRecursionLimit = 1456,
    ///<summary></summary>
    ///<remarks>ER_SP_PROC_TABLE_CORRUPT</remarks>
    StoredProcedureTableCorrupt = 1457,
    ///<summary></summary>
    ///<remarks>ER_SP_WRONG_NAME</remarks>
    StoredProcedureWrongName = 1458,
    ///<summary></summary>
    ///<remarks>ER_TABLE_NEEDS_UPGRADE</remarks>
    TableNeedsUpgrade = 1459,
    ///<summary></summary>
    ///<remarks>ER_SP_NO_AGGREGATE</remarks>
    StoredProcedureNoAggregate = 1460,
    ///<summary></summary>
    ///<remarks>ER_MAX_PREPARED_STMT_COUNT_REACHED</remarks>
    MaxPreparedStatementCountReached = 1461,
    ///<summary></summary>
    ///<remarks>ER_VIEW_RECURSIVE</remarks>
    ViewRecursive = 1462,
    ///<summary></summary>
    ///<remarks>ER_NON_GROUPING_FIELD_USED</remarks>
    NonGroupingFieldUsed = 1463,
    ///<summary></summary>
    ///<remarks>ER_TABLE_CANT_HANDLE_SPKEYS</remarks>
    TableCannotHandleSpatialKeys = 1464,
    ///<summary></summary>
    ///<remarks>ER_NO_TRIGGERS_ON_SYSTEM_SCHEMA</remarks>
    NoTriggersOnSystemSchema = 1465,
    ///<summary></summary>
    ///<remarks>ER_REMOVED_SPACES</remarks>
    RemovedSpaces = 1466,
    ///<summary></summary>
    ///<remarks>ER_AUTOINC_READ_FAILED</remarks>
    AutoIncrementReadFailed = 1467,
    ///<summary></summary>
    ///<remarks>ER_USERNAME</remarks>
    UserNameError = 1468,
    ///<summary></summary>
    ///<remarks>ER_HOSTNAME</remarks>
    HostNameError = 1469,
    ///<summary></summary>
    ///<remarks>ER_WRONG_STRING_LENGTH</remarks>
    WrongStringLength = 1470,
    ///<summary></summary>
    ///<remarks>ER_NON_INSERTABLE_TABLE</remarks>
    NonInsertableTable = 1471,
    ///<summary></summary>
    ///<remarks>ER_ADMIN_WRONG_MRG_TABLE</remarks>
    AdminWrongMergeTable = 1472,
    ///<summary></summary>
    ///<remarks>ER_TOO_HIGH_LEVEL_OF_NESTING_FOR_SELECT</remarks>
    TooHighLevelOfNestingForSelect = 1473,
    ///<summary></summary>
    ///<remarks>ER_NAME_BECOMES_EMPTY</remarks>
    NameBecomesEmpty = 1474,
    ///<summary></summary>
    ///<remarks>ER_AMBIGUOUS_FIELD_TERM</remarks>
    AmbiguousFieldTerm = 1475,
    ///<summary></summary>
    ///<remarks>ER_FOREIGN_SERVER_EXISTS</remarks>
    ForeignServerExists = 1476,
    ///<summary></summary>
    ///<remarks>ER_FOREIGN_SERVER_DOESNT_EXIST</remarks>
    ForeignServerDoesNotExist = 1477,
    ///<summary></summary>
    ///<remarks>ER_ILLEGAL_HA_CREATE_OPTION</remarks>
    IllegalHACreateOption = 1478,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_REQUIRES_VALUES_ERROR</remarks>
    PartitionRequiresValues = 1479,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_WRONG_VALUES_ERROR</remarks>
    PartitionWrongValues = 1480,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_MAXVALUE_ERROR</remarks>
    PartitionMaxValue = 1481,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_SUBPARTITION_ERROR</remarks>
    PartitionSubPartition = 1482,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_SUBPART_MIX_ERROR</remarks>
    PartitionSubPartMix = 1483,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_WRONG_NO_PART_ERROR</remarks>
    PartitionWrongNoPart = 1484,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_WRONG_NO_SUBPART_ERROR</remarks>
    PartitionWrongNoSubPart = 1485,
    ///<summary></summary>
    ///<remarks>ER_WRONG_EXPR_IN_PARTITION_FUNC_ERROR</remarks>
    WrongExpressionInParitionFunction = 1486,
    ///<summary></summary>
    ///<remarks>ER_NO_CONST_EXPR_IN_RANGE_OR_LIST_ERROR</remarks>
    NoConstantExpressionInRangeOrListError = 1487,
    ///<summary></summary>
    ///<remarks>ER_FIELD_NOT_FOUND_PART_ERROR</remarks>
    FieldNotFoundPartitionErrror = 1488,
    ///<summary></summary>
    ///<remarks>ER_LIST_OF_FIELDS_ONLY_IN_HASH_ERROR</remarks>
    ListOfFieldsOnlyInHash = 1489,
    ///<summary></summary>
    ///<remarks>ER_INCONSISTENT_PARTITION_INFO_ERROR</remarks>
    InconsistentPartitionInfo = 1490,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_FUNC_NOT_ALLOWED_ERROR</remarks>
    PartitionFunctionNotAllowed = 1491,
    ///<summary></summary>
    ///<remarks>ER_PARTITIONS_MUST_BE_DEFINED_ERROR</remarks>
    PartitionsMustBeDefined = 1492,
    ///<summary></summary>
    ///<remarks>ER_RANGE_NOT_INCREASING_ERROR</remarks>
    RangeNotIncreasing = 1493,
    ///<summary></summary>
    ///<remarks>ER_INCONSISTENT_TYPE_OF_FUNCTIONS_ERROR</remarks>
    InconsistentTypeOfFunctions = 1494,
    ///<summary></summary>
    ///<remarks>ER_MULTIPLE_DEF_CONST_IN_LIST_PART_ERROR</remarks>
    MultipleDefinitionsConstantInListPartition = 1495,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_ENTRY_ERROR</remarks>
    PartitionEntryError = 1496,
    ///<summary></summary>
    ///<remarks>ER_MIX_HANDLER_ERROR</remarks>
    MixHandlerError = 1497,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_NOT_DEFINED_ERROR</remarks>
    PartitionNotDefined = 1498,
    ///<summary></summary>
    ///<remarks>ER_TOO_MANY_PARTITIONS_ERROR</remarks>
    TooManyPartitions = 1499,
    ///<summary></summary>
    ///<remarks>ER_SUBPARTITION_ERROR</remarks>
    SubPartitionError = 1500,
    ///<summary></summary>
    ///<remarks>ER_CANT_CREATE_HANDLER_FILE</remarks>
    CannotCreateHandlerFile = 1501,
    ///<summary></summary>
    ///<remarks>ER_BLOB_FIELD_IN_PART_FUNC_ERROR</remarks>
    BlobFieldInPartitionFunction = 1502,
    ///<summary></summary>
    ///<remarks>ER_UNIQUE_KEY_NEED_ALL_FIELDS_IN_PF</remarks>
    UniqueKeyNeedAllFieldsInPartitioningFunction = 1503,
    ///<summary></summary>
    ///<remarks>ER_NO_PARTS_ERROR</remarks>
    NoPartitions = 1504,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_MGMT_ON_NONPARTITIONED</remarks>
    PartitionManagementOnNoPartitioned = 1505,
    ///<summary></summary>
    ///<remarks>ER_FOREIGN_KEY_ON_PARTITIONED</remarks>
    ForeignKeyOnPartitioned = 1506,
    ///<summary></summary>
    ///<remarks>ER_DROP_PARTITION_NON_EXISTENT</remarks>
    DropPartitionNonExistent = 1507,
    ///<summary></summary>
    ///<remarks>ER_DROP_LAST_PARTITION</remarks>
    DropLastPartition = 1508,
    ///<summary></summary>
    ///<remarks>ER_COALESCE_ONLY_ON_HASH_PARTITION</remarks>
    CoalesceOnlyOnHashPartition = 1509,
    ///<summary></summary>
    ///<remarks>ER_REORG_HASH_ONLY_ON_SAME_NO</remarks>
    ReorganizeHashOnlyOnSameNumber = 1510,
    ///<summary></summary>
    ///<remarks>ER_REORG_NO_PARAM_ERROR</remarks>
    ReorganizeNoParameter = 1511,
    ///<summary></summary>
    ///<remarks>ER_ONLY_ON_RANGE_LIST_PARTITION</remarks>
    OnlyOnRangeListPartition = 1512,
    ///<summary></summary>
    ///<remarks>ER_ADD_PARTITION_SUBPART_ERROR</remarks>
    AddPartitionSubPartition = 1513,
    ///<summary></summary>
    ///<remarks>ER_ADD_PARTITION_NO_NEW_PARTITION</remarks>
    AddPartitionNoNewPartition = 1514,
    ///<summary></summary>
    ///<remarks>ER_COALESCE_PARTITION_NO_PARTITION</remarks>
    CoalescePartitionNoPartition = 1515,
    ///<summary></summary>
    ///<remarks>ER_REORG_PARTITION_NOT_EXIST</remarks>
    ReorganizePartitionNotExist = 1516,
    ///<summary></summary>
    ///<remarks>ER_SAME_NAME_PARTITION</remarks>
    SameNamePartition = 1517,
    ///<summary></summary>
    ///<remarks>ER_NO_BINLOG_ERROR</remarks>
    NoBinLog = 1518,
    ///<summary></summary>
    ///<remarks>ER_CONSECUTIVE_REORG_PARTITIONS</remarks>
    ConsecutiveReorganizePartitions = 1519,
    ///<summary></summary>
    ///<remarks>ER_REORG_OUTSIDE_RANGE</remarks>
    ReorganizeOutsideRange = 1520,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_FUNCTION_FAILURE</remarks>
    PartitionFunctionFailure = 1521,
    ///<summary></summary>
    ///<remarks>ER_PART_STATE_ERROR</remarks>
    PartitionStateError = 1522,
    ///<summary></summary>
    ///<remarks>ER_LIMITED_PART_RANGE</remarks>
    LimitedPartitionRange = 1523,
    ///<summary></summary>
    ///<remarks>ER_PLUGIN_IS_NOT_LOADED</remarks>
    PluginIsNotLoaded = 1524,
    ///<summary></summary>
    ///<remarks>ER_WRONG_VALUE</remarks>
    WrongValue = 1525,
    ///<summary></summary>
    ///<remarks>ER_NO_PARTITION_FOR_GIVEN_VALUE</remarks>
    NoPartitionForGivenValue = 1526,
    ///<summary></summary>
    ///<remarks>ER_FILEGROUP_OPTION_ONLY_ONCE</remarks>
    FileGroupOptionOnlyOnce = 1527,
    ///<summary></summary>
    ///<remarks>ER_CREATE_FILEGROUP_FAILED</remarks>
    CreateFileGroupFailed = 1528,
    ///<summary></summary>
    ///<remarks>ER_DROP_FILEGROUP_FAILED</remarks>
    DropFileGroupFailed = 1529,
    ///<summary></summary>
    ///<remarks>ER_TABLESPACE_AUTO_EXTEND_ERROR</remarks>
    TableSpaceAutoExtend = 1530,
    ///<summary></summary>
    ///<remarks>ER_WRONG_SIZE_NUMBER</remarks>
    WrongSizeNumber = 1531,
    ///<summary></summary>
    ///<remarks>ER_SIZE_OVERFLOW_ERROR</remarks>
    SizeOverflow = 1532,
    ///<summary></summary>
    ///<remarks>ER_ALTER_FILEGROUP_FAILED</remarks>
    AlterFileGroupFailed = 1533,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_ROW_LOGGING_FAILED</remarks>
    BinLogRowLogginFailed = 1534,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_ROW_WRONG_TABLE_DEF</remarks>
    BinLogRowWrongTableDefinition = 1535,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_ROW_RBR_TO_SBR</remarks>
    BinLogRowRBRToSBR = 1536,
    ///<summary></summary>
    ///<remarks>ER_EVENT_ALREADY_EXISTS</remarks>
    EventAlreadyExists = 1537,
    ///<summary></summary>
    ///<remarks>ER_EVENT_STORE_FAILED</remarks>
    EventStoreFailed = 1538,
    ///<summary></summary>
    ///<remarks>ER_EVENT_DOES_NOT_EXIST</remarks>
    EventDoesNotExist = 1539,
    ///<summary></summary>
    ///<remarks>ER_EVENT_CANT_ALTER</remarks>
    EventCannotAlter = 1540,
    ///<summary></summary>
    ///<remarks>ER_EVENT_DROP_FAILED</remarks>
    EventDropFailed = 1541,
    ///<summary></summary>
    ///<remarks>ER_EVENT_INTERVAL_NOT_POSITIVE_OR_TOO_BIG</remarks>
    EventIntervalNotPositiveOrTooBig = 1542,
    ///<summary></summary>
    ///<remarks>ER_EVENT_ENDS_BEFORE_STARTS</remarks>
    EventEndsBeforeStarts = 1543,
    ///<summary></summary>
    ///<remarks>ER_EVENT_EXEC_TIME_IN_THE_PAST</remarks>
    EventExecTimeInThePast = 1544,
    ///<summary></summary>
    ///<remarks>ER_EVENT_OPEN_TABLE_FAILED</remarks>
    EventOpenTableFailed = 1545,
    ///<summary></summary>
    ///<remarks>ER_EVENT_NEITHER_M_EXPR_NOR_M_AT</remarks>
    EventNeitherMExpresssionNorMAt = 1546,
    ///<summary></summary>
    ///<remarks>ER_COL_COUNT_DOESNT_MATCH_CORRUPTED</remarks>
    ColumnCountDoesNotMatchCorrupted = 1547,
    ///<summary></summary>
    ///<remarks>ER_CANNOT_LOAD_FROM_TABLE</remarks>
    CannotLoadFromTable = 1548,
    ///<summary></summary>
    ///<remarks>ER_EVENT_CANNOT_DELETE</remarks>
    EventCannotDelete = 1549,
    ///<summary></summary>
    ///<remarks>ER_EVENT_COMPILE_ERROR</remarks>
    EventCompileError = 1550,
    ///<summary></summary>
    ///<remarks>ER_EVENT_SAME_NAME</remarks>
    EventSameName = 1551,
    ///<summary></summary>
    ///<remarks>ER_EVENT_DATA_TOO_LONG</remarks>
    EventDataTooLong = 1552,
    ///<summary></summary>
    ///<remarks>ER_DROP_INDEX_FK</remarks>
    DropIndexForeignKey = 1553,
    ///<summary></summary>
    ///<remarks>ER_WARN_DEPRECATED_SYNTAX_WITH_VER</remarks>
    WarningDeprecatedSyntaxWithVersion = 1554,
    ///<summary></summary>
    ///<remarks>ER_CANT_WRITE_LOCK_LOG_TABLE</remarks>
    CannotWriteLockLogTable = 1555,
    ///<summary></summary>
    ///<remarks>ER_CANT_LOCK_LOG_TABLE</remarks>
    CannotLockLogTable = 1556,
    ///<summary></summary>
    ///<remarks>ER_FOREIGN_DUPLICATE_KEY</remarks>
    ForeignDuplicateKey = 1557,
    ///<summary></summary>
    ///<remarks>ER_COL_COUNT_DOESNT_MATCH_PLEASE_UPDATE</remarks>
    ColumnCountDoesNotMatchPleaseUpdate = 1558,
    ///<summary></summary>
    ///<remarks>ER_TEMP_TABLE_PREVENTS_SWITCH_OUT_OF_RBR</remarks>
    TemoraryTablePreventSwitchOutOfRBR = 1559,
    ///<summary></summary>
    ///<remarks>ER_STORED_FUNCTION_PREVENTS_SWITCH_BINLOG_FORMAT</remarks>
    StoredFunctionPreventsSwitchBinLogFormat = 1560,
    ///<summary></summary>
    ///<remarks>ER_NDB_CANT_SWITCH_BINLOG_FORMAT</remarks>
    NDBCannotSwitchBinLogFormat = 1561,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_NO_TEMPORARY</remarks>
    PartitionNoTemporary = 1562,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_CONST_DOMAIN_ERROR</remarks>
    PartitionConstantDomain = 1563,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_FUNCTION_IS_NOT_ALLOWED</remarks>
    PartitionFunctionIsNotAllowed = 1564,
    ///<summary></summary>
    ///<remarks>ER_DDL_LOG_ERROR</remarks>
    DDLLogError = 1565,
    ///<summary></summary>
    ///<remarks>ER_NULL_IN_VALUES_LESS_THAN</remarks>
    NullInValuesLessThan = 1566,
    ///<summary></summary>
    ///<remarks>ER_WRONG_PARTITION_NAME</remarks>
    WrongPartitionName = 1567,
    ///<summary></summary>
    ///<remarks>ER_CANT_CHANGE_TX_ISOLATION</remarks>
    CannotChangeTransactionIsolation = 1568,
    ///<summary></summary>
    ///<remarks>ER_DUP_ENTRY_AUTOINCREMENT_CASE</remarks>
    DuplicateEntryAutoIncrementCase = 1569,
    ///<summary></summary>
    ///<remarks>ER_EVENT_MODIFY_QUEUE_ERROR</remarks>
    EventModifyQueueError = 1570,
    ///<summary></summary>
    ///<remarks>ER_EVENT_SET_VAR_ERROR</remarks>
    EventSetVariableError = 1571,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_MERGE_ERROR</remarks>
    PartitionMergeError = 1572,
    ///<summary></summary>
    ///<remarks>ER_CANT_ACTIVATE_LOG</remarks>
    CannotActivateLog = 1573,
    ///<summary></summary>
    ///<remarks>ER_RBR_NOT_AVAILABLE</remarks>
    RBRNotAvailable = 1574,
    ///<summary></summary>
    ///<remarks>ER_BASE64_DECODE_ERROR</remarks>
    Base64DecodeError = 1575,
    ///<summary></summary>
    ///<remarks>ER_EVENT_RECURSION_FORBIDDEN</remarks>
    EventRecursionForbidden = 1576,
    ///<summary></summary>
    ///<remarks>ER_EVENTS_DB_ERROR</remarks>
    EventsDatabaseError = 1577,
    ///<summary></summary>
    ///<remarks>ER_ONLY_INTEGERS_ALLOWED</remarks>
    OnlyIntegersAllowed = 1578,
    ///<summary></summary>
    ///<remarks>ER_UNSUPORTED_LOG_ENGINE</remarks>
    UnsupportedLogEngine = 1579,
    ///<summary></summary>
    ///<remarks>ER_BAD_LOG_STATEMENT</remarks>
    BadLogStatement = 1580,
    ///<summary></summary>
    ///<remarks>ER_CANT_RENAME_LOG_TABLE</remarks>
    CannotRenameLogTable = 1581,
    ///<summary></summary>
    ///<remarks>ER_WRONG_PARAMCOUNT_TO_NATIVE_FCT</remarks>
    WrongParameterCountToNativeFCT = 1582,
    ///<summary></summary>
    ///<remarks>ER_WRONG_PARAMETERS_TO_NATIVE_FCT</remarks>
    WrongParametersToNativeFCT = 1583,
    ///<summary></summary>
    ///<remarks>ER_WRONG_PARAMETERS_TO_STORED_FCT</remarks>
    WrongParametersToStoredFCT = 1584,
    ///<summary></summary>
    ///<remarks>ER_NATIVE_FCT_NAME_COLLISION</remarks>
    NativeFCTNameCollision = 1585,
    ///<summary></summary>
    ///<remarks>ER_DUP_ENTRY_WITH_KEY_NAME</remarks>
    DuplicateEntryWithKeyName = 1586,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_PURGE_EMFILE</remarks>
    BinLogPurgeEMFile = 1587,
    ///<summary></summary>
    ///<remarks>ER_EVENT_CANNOT_CREATE_IN_THE_PAST</remarks>
    EventCannotCreateInThePast = 1588,
    ///<summary></summary>
    ///<remarks>ER_EVENT_CANNOT_ALTER_IN_THE_PAST</remarks>
    EventCannotAlterInThePast = 1589,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_INCIDENT</remarks>
    SlaveIncident = 1590,
    ///<summary></summary>
    ///<remarks>ER_NO_PARTITION_FOR_GIVEN_VALUE_SILENT</remarks>
    NoPartitionForGivenValueSilent = 1591,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_UNSAFE_STATEMENT</remarks>
    BinLogUnsafeStatement = 1592,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_FATAL_ERROR</remarks>
    SlaveFatalError = 1593,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_RELAY_LOG_READ_FAILURE</remarks>
    SlaveRelayLogReadFailure = 1594,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_RELAY_LOG_WRITE_FAILURE</remarks>
    SlaveRelayLogWriteFailure = 1595,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_CREATE_EVENT_FAILURE</remarks>
    SlaveCreateEventFailure = 1596,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_MASTER_COM_FAILURE</remarks>
    SlaveMasterComFailure = 1597,
    ///<summary></summary>
    ///<remarks>ER_BINLOG_LOGGING_IMPOSSIBLE</remarks>
    BinLogLoggingImpossible = 1598,
    ///<summary></summary>
    ///<remarks>ER_VIEW_NO_CREATION_CTX</remarks>
    ViewNoCreationContext = 1599,
    ///<summary></summary>
    ///<remarks>ER_VIEW_INVALID_CREATION_CTX</remarks>
    ViewInvalidCreationContext = 1600,
    ///<summary></summary>
    ///<remarks>ER_SR_INVALID_CREATION_CTX</remarks>
    StoredRoutineInvalidCreateionContext = 1601,
    ///<summary></summary>
    ///<remarks>ER_TRG_CORRUPTED_FILE</remarks>
    TiggerCorruptedFile = 1602,
    ///<summary></summary>
    ///<remarks>ER_TRG_NO_CREATION_CTX</remarks>
    TriggerNoCreationContext = 1603,
    ///<summary></summary>
    ///<remarks>ER_TRG_INVALID_CREATION_CTX</remarks>
    TriggerInvalidCreationContext = 1604,
    ///<summary></summary>
    ///<remarks>ER_EVENT_INVALID_CREATION_CTX</remarks>
    EventInvalidCreationContext = 1605,
    ///<summary></summary>
    ///<remarks>ER_TRG_CANT_OPEN_TABLE</remarks>
    TriggerCannotOpenTable = 1606,
    ///<summary></summary>
    ///<remarks>ER_CANT_CREATE_SROUTINE</remarks>
    CannoCreateSubRoutine = 1607,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_AMBIGOUS_EXEC_MODE</remarks>
    SlaveAmbiguousExecMode = 1608,
    ///<summary></summary>
    ///<remarks>ER_NO_FORMAT_DESCRIPTION_EVENT_BEFORE_BINLOG_STATEMENT</remarks>
    NoFormatDescriptionEventBeforeBinLogStatement = 1609,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_CORRUPT_EVENT</remarks>
    SlaveCorruptEvent = 1610,
    ///<summary></summary>
    ///<remarks>ER_LOAD_DATA_INVALID_COLUMN</remarks>
    LoadDataInvalidColumn = 1611,
    ///<summary></summary>
    ///<remarks>ER_LOG_PURGE_NO_FILE</remarks>
    LogPurgeNoFile = 1612,
    ///<summary></summary>
    ///<remarks>ER_XA_RBTIMEOUT</remarks>
    XARBTimeout = 1613,
    ///<summary></summary>
    ///<remarks>ER_XA_RBDEADLOCK</remarks>
    XARBDeadlock = 1614,
    ///<summary></summary>
    ///<remarks>ER_NEED_REPREPARE</remarks>
    NeedRePrepare = 1615,
    ///<summary></summary>
    ///<remarks>ER_DELAYED_NOT_SUPPORTED</remarks>
    DelayedNotSupported = 1616,
    ///<summary></summary>
    ///<remarks>WARN_NO_MASTER_INFO</remarks>
    WarningNoMasterInfo = 1617,
    ///<summary></summary>
    ///<remarks>WARN_OPTION_IGNORED</remarks>
    WarningOptionIgnored = 1618,
    ///<summary></summary>
    ///<remarks>WARN_PLUGIN_DELETE_BUILTIN</remarks>
    WarningPluginDeleteBuiltIn = 1619,
    ///<summary></summary>
    ///<remarks>WARN_PLUGIN_BUSY</remarks>
    WarningPluginBusy = 1620,
    ///<summary></summary>
    ///<remarks>ER_VARIABLE_IS_READONLY</remarks>
    VariableIsReadonly = 1621,
    ///<summary></summary>
    ///<remarks>ER_WARN_ENGINE_TRANSACTION_ROLLBACK</remarks>
    WarningEngineTransactionRollback = 1622,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_HEARTBEAT_FAILURE</remarks>
    SlaveHeartbeatFailure = 1623,
    ///<summary></summary>
    ///<remarks>ER_SLAVE_HEARTBEAT_VALUE_OUT_OF_RANGE</remarks>
    SlaveHeartbeatValueOutOfRange = 1624,
    ///<summary></summary>
    ///<remarks>ER_NDB_REPLICATION_SCHEMA_ERROR</remarks>
    NDBReplicationSchemaError = 1625,
    ///<summary></summary>
    ///<remarks>ER_CONFLICT_FN_PARSE_ERROR</remarks>
    ConflictFunctionParseError = 1626,
    ///<summary></summary>
    ///<remarks>ER_EXCEPTIONS_WRITE_ERROR</remarks>
    ExcepionsWriteError = 1627,
    ///<summary></summary>
    ///<remarks>ER_TOO_LONG_TABLE_COMMENT</remarks>
    TooLongTableComment = 1628,
    ///<summary></summary>
    ///<remarks>ER_TOO_LONG_FIELD_COMMENT</remarks>
    TooLongFieldComment = 1629,
    ///<summary></summary>
    ///<remarks>ER_FUNC_INEXISTENT_NAME_COLLISION</remarks>
    FunctionInExistentNameCollision = 1630,
    ///<summary></summary>
    ///<remarks>ER_DATABASE_NAME</remarks>
    DatabaseNameError = 1631,
    ///<summary></summary>
    ///<remarks>ER_TABLE_NAME</remarks>
    TableNameErrror = 1632,
    ///<summary></summary>
    ///<remarks>ER_PARTITION_NAME</remarks>
    PartitionNameError = 1633,
    ///<summary></summary>
    ///<remarks>ER_SUBPARTITION_NAME</remarks>
    SubPartitionNameError = 1634,
    ///<summary></summary>
    ///<remarks>ER_TEMPORARY_NAME</remarks>
    TemporaryNameError = 1635,
    ///<summary></summary>
    ///<remarks>ER_RENAMED_NAME</remarks>
    RenamedNameError = 1636,
    ///<summary></summary>
    ///<remarks>ER_TOO_MANY_CONCURRENT_TRXS</remarks>
    TooManyConcurrentTransactions = 1637,
    ///<summary></summary>
    ///<remarks>WARN_NON_ASCII_SEPARATOR_NOT_IMPLEMENTED</remarks>
    WarningNonASCIISeparatorNotImplemented = 1638,
    ///<summary></summary>
    ///<remarks>ER_DEBUG_SYNC_TIMEOUT</remarks>
    DebugSyncTimeout = 1639,
    ///<summary></summary>
    ///<remarks>ER_DEBUG_SYNC_HIT_LIMIT</remarks>
    DebugSyncHitLimit = 1640,
    ///<summary></summary>
    ///<remarks>ER_ERROR_LAST</remarks>
    ErrorLast = 1640
  }
}
