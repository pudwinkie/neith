
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = ../build/bin/Debug/Smdn.Net.Imap4.Client.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.dll \
	../build/bin/Debug/Smdn.Net.Imap4.dll \
	../build/bin/Debug/Smdn.Security.Sasl.dll
BUILD_DIR = ../build/bin/Debug/

SMDN_DLL_SOURCE=../build/bin/Debug/Smdn.dll
SMDN_NET_IMAP4_DLL_SOURCE=../build/bin/Debug/Smdn.Net.Imap4.dll
SMDN_SECURITY_SASL_DLL_SOURCE=../build/bin/Debug/Smdn.Security.Sasl.dll
SMDN_NET_IMAP4_CLIENT_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Net.Imap4.Client.dll.mdb
SMDN_NET_IMAP4_CLIENT_DLL_MDB=$(BUILD_DIR)/Smdn.Net.Imap4.Client.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:TRACE"
ASSEMBLY = ../build/bin/Release/Smdn.Net.Imap4.Client.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.dll \
	../build/bin/Release/Smdn.Net.Imap4.dll \
	../build/bin/Release/Smdn.Security.Sasl.dll
BUILD_DIR = ../build/bin/Release/

SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_NET_IMAP4_DLL_SOURCE=../build/bin/Release/Smdn.Net.Imap4.dll
SMDN_SECURITY_SASL_DLL_SOURCE=../build/bin/Release/Smdn.Security.Sasl.dll
SMDN_NET_IMAP4_CLIENT_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_DLL) \
	$(SMDN_NET_IMAP4_DLL) \
	$(SMDN_SECURITY_SASL_DLL) \
	$(SMDN_NET_IMAP4_CLIENT_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_NET_IMAP4_CLIENT_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Net.Imap4.WebClient/IImapSessionProfile.cs \
	Smdn.Net.Imap4.WebClient/ImapAppendMessageBodyStream.cs \
	Smdn.Net.Imap4.WebClient/ImapFetchMessageBodyStream.cs \
	Smdn.Net.Imap4.WebClient/ImapFetchMessageWebRequest.cs \
	Smdn.Net.Imap4.WebClient/ImapMailboxWebRequest.cs \
	Smdn.Net.Imap4.WebClient/ImapSearchMessageWebRequest.cs \
	Smdn.Net.Imap4.WebClient/ImapServerWebRequest.cs \
	Smdn.Net.Imap4.WebClient/ImapSessionManager.cs \
	Smdn.Net.Imap4.WebClient/ImapThreadTree.cs \
	Smdn.Net.Imap4.WebClient/ImapWebRequest.cs \
	Smdn.Net.Imap4.WebClient/ImapWebRequestMethods.cs \
	Smdn.Net.Imap4.WebClient/ImapWebResponse.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapCommand.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapCommandContinuationRequest.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapCommandSender.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapConnection.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapDataResponse.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapDataResponseConverter.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapDataResponseType.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapMalformedResponseException.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapResponse.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapResponseCode.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapResponseReceiver.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapResponseText.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapResponseTextConverter.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapStatusResponse.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapTaggedStatusResponse.cs \
	Smdn.Net.Imap4.Protocol.Client/ImapUntaggedStatusResponse.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/AppendTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/AuthenticateTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/CapabilityTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/CheckTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/CloseTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/CompressTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/CopyTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/CreateTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/DeleteTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/EnableTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/ExamineTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/ExpungeTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/FetchTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/GenericCommandTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/GreetingTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/IdTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/IdleTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/ListTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/ListTransactionBase.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/LoginTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/LogoutTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/LsubTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/NamespaceTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/NoOpTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/NoOpTransactionBase.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/RListTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/RLsubTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/RenameTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/SearchTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/SearchTransactionBase.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/SelectTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/SelectTransactionBase.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/SendCommandTransactionBase.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/SortTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/StartTlsTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/StatusTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/StoreTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/SubscribeTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/SubscribeTransactionBase.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/ThreadTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/UnselectTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction.BuiltIn/UnsubscribeTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction/IImapTransaction.cs \
	Smdn.Net.Imap4.Client.Transaction/ImapTransactionBase.cs \
	Smdn.Net.Imap4.Client.Transaction/ImapTransactionRequest.cs \
	Smdn.Net.Imap4.Client.Transaction/ImapTransactionResult.cs \
	Smdn.Net.Imap4.Client.Transaction/ImapTransactionResultCode.cs \
	Smdn.Net.Imap4.Client.Transaction/ImapTransactionState.cs \
	Smdn.Net.Imap4.Client.Session/ImapCapabilityResponseProcessor.cs \
	Smdn.Net.Imap4.Client.Session/ImapIncapableException.cs \
	Smdn.Net.Imap4.Client.Session/ImapLoginReferralException.cs \
	Smdn.Net.Imap4.Client.Session/ImapMailboxManager.cs \
	Smdn.Net.Imap4.Client.Session/ImapMailboxReferralException.cs \
	Smdn.Net.Imap4.Client.Session/ImapReferralException.cs \
	Smdn.Net.Imap4.Client.Session/ImapReferralResponseProcessor.cs \
	Smdn.Net.Imap4.Client.Session/ImapResponseProcessorBase.cs \
	Smdn.Net.Imap4.Client.Session/ImapSession.cs \
	Smdn.Net.Imap4.Client.Session/ImapUpdatedStatusResponseProcessor.cs \
	Smdn.Net.Imap4/ImapAppendedUidSet.cs \
	Smdn.Net.Imap4/ImapCopiedUidSet.cs \
	Smdn.Net.Imap4/ImapFetchDataItem.cs \
	Smdn.Net.Imap4/ImapMailbox.cs \
	Smdn.Net.Imap4/ImapSearchCriteria.cs \
	Smdn.Net.Imap4/ImapSearchResultOptions.cs \
	Smdn.Net.Imap4/ImapSortCriteria.cs \
	Smdn.Net.Imap4/ImapStatusDataItem.cs \
	Smdn.Net.Imap4/ImapStatusUpdatedEventArgs.cs \
	Smdn.Net.Imap4/ImapStoreDataItem.cs \
	Smdn.Net.Imap4/ImapUpdatedStatus.cs \
	Smdn.Net.Imap4/ImapUpdatedStatusKind.cs \
	Smdn.Net.Imap4.Client/Trace.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Net.Imap4.Client \
	smdn.net.imap4.client.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_NET_IMAP4_DLL = $(BUILD_DIR)/Smdn.Net.Imap4.dll
SMDN_SECURITY_SASL_DLL = $(BUILD_DIR)/Smdn.Security.Sasl.dll
SMDN_NET_IMAP4_CLIENT_PC = $(BUILD_DIR)/smdn.net.imap4.client.pc

$(eval $(call emit-deploy-wrapper,SMDN_NET_IMAP4_CLIENT_PC,smdn.net.imap4.client.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
