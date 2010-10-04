
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG;TRACE;DUMP"
ASSEMBLY = ../build/bin/Debug/Smdn.Net.Pop3.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../build/bin/Debug/

SMDN_NET_POP3_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Net.Pop3.dll.mdb
SMDN_NET_POP3_DLL_MDB=$(BUILD_DIR)/Smdn.Net.Pop3.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:TRACE"
ASSEMBLY = ../build/bin/Release/Smdn.Net.Pop3.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../build/bin/Release/

SMDN_NET_POP3_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_NET_POP3_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_NET_POP3_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Smdn.Net.Pop3/Trace.cs \
	Smdn.Net.Pop3/PopUri.cs \
	Smdn.Net.Pop3/PopUriForm.cs \
	Smdn.Net.Pop3/PopException.cs \
	Smdn.Net.Pop3/PopConverter.cs \
	Smdn.Net.Pop3/PopAuthenticationType.cs \
	Smdn.Net.Pop3/PopCapability.cs \
	Smdn.Net.Pop3/PopKnownCapabilities.cs \
	Smdn.Net.Pop3/PopSeverInfo.cs \
	Smdn.Net.Pop3/PopCreateAuthenticatedStreamCallback.cs \
	Smdn.Net.Pop3/PopDropList.cs \
	Smdn.Net.Pop3/PopScanList.cs \
	Smdn.Net.Pop3/PopMessage.cs \
	Smdn.Net.Pop3/PopUniqueIdList.cs \
	Smdn.Net.Pop3/PopMailbox.cs \
	Smdn.Net.Pop3.Client/PopClient.cs \
	Smdn.Net.Pop3.Client/PopClientAgent.cs \
	Smdn.Net.Pop3.Client/PopClientException.cs \
	Smdn.Net.Pop3.Client/PopDisconnectedException.cs \
	Smdn.Net.Pop3.Protocol/PopConnection.cs \
	Smdn.Net.Pop3.Protocol/PopContinuationRequest.cs \
	Smdn.Net.Pop3.Protocol/PopFollowingResponse.cs \
	Smdn.Net.Pop3.Protocol/PopProtocolCommand.cs \
	Smdn.Net.Pop3.Protocol/PopProtocolResponse.cs \
	Smdn.Net.Pop3.Protocol/PopProtocolResponseParser.cs \
	Smdn.Net.Pop3.Protocol/PopProtocolStatus.cs \
	Smdn.Net.Pop3.Protocol/PopStatusResponse.cs \
	Smdn.Net.Pop3.Protocol/PopTerminationResponse.cs \
	Smdn.Net.Pop3.Session/PopSession.cs \
	Smdn.Net.Pop3.Session/PopSessionAgent.cs \
	Smdn.Net.Pop3.Session/PopSessionState.cs \
	Smdn.Net.Pop3.Transaction/PopCapabilityResponse.cs \
	Smdn.Net.Pop3.Transaction/PopDropListResponse.cs \
	Smdn.Net.Pop3.Transaction/PopMessageResponse.cs \
	Smdn.Net.Pop3.Transaction/PopRequest.cs \
	Smdn.Net.Pop3.Transaction/PopResponse.cs \
	Smdn.Net.Pop3.Transaction/PopScanListResponse.cs \
	Smdn.Net.Pop3.Transaction/PopStlsRequest.cs \
	Smdn.Net.Pop3.Transaction/PopTransaction.cs \
	Smdn.Net.Pop3.Transaction/PopTransactionAgent.cs \
	Smdn.Net.Pop3.Transaction/PopTransactionMethod.cs \
	Smdn.Net.Pop3.Transaction/PopTransactionStatus.cs \
	Smdn.Net.Pop3.Transaction/PopUniqueIdListResponse.cs \
	AssemblyInfo.cs \
	Smdn.Net.Pop3.Transaction.Transactions/ApopTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/AuthTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/CapaTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/CompositeTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/DeleTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/GreetingTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/ListTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/NoOpTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/PassTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/QuitTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/RetrTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/RsetTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/StatTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/StlsTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/TopTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/TransactionBase.cs \
	Smdn.Net.Pop3.Transaction.Transactions/UidlTransaction.cs \
	Smdn.Net.Pop3.Transaction.Transactions/UserTransaction.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	smdn.net.pop3.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_NET_POP3_PC = $(BUILD_DIR)/smdn.net.pop3.pc

$(eval $(call emit-deploy-wrapper,SMDN_NET_POP3_PC,smdn.net.pop3.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
