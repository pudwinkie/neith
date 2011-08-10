
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = ../build/bin/Debug/Smdn.Net.Imap4.Server.dll
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
SMDN_NET_IMAP4_SERVER_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Net.Imap4.Server.dll.mdb
SMDN_NET_IMAP4_SERVER_DLL_MDB=$(BUILD_DIR)/Smdn.Net.Imap4.Server.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:TRACE"
ASSEMBLY = ../build/bin/Release/Smdn.Net.Imap4.Server.dll
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
SMDN_NET_IMAP4_SERVER_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_DLL) \
	$(SMDN_NET_IMAP4_DLL) \
	$(SMDN_SECURITY_SASL_DLL) \
	$(SMDN_NET_IMAP4_SERVER_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_NET_IMAP4_SERVER_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Net.Imap4.Server/Trace.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapConnection.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapResponseSender.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapCommandReceiver.cs \
	Smdn.Net.Imap4.Server.Session/ImapSession.cs \
	Smdn.Net.Imap4.Server.Session/ImapServer.cs \
	Smdn.Net.Imap4.Server.Session/ImapSslServer.cs \
	Smdn.Net.Imap4.Server.Session/IImapServer.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapResponse.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapTaggedStatusResponse.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapUntaggedStatusResponse.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapDataResponse.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapCommandContinuationRequest.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapStatusResponse.cs \
	Smdn.Net.Imap4.Server.Transaction.BuiltIn/GreetingTransaction.cs \
	Smdn.Net.Imap4.Server.Transaction/IImapTransaction.cs \
	Smdn.Net.Imap4.Server.Transaction/ImapTransactionBase.cs \
	Smdn.Net.Imap4.Server.Transaction/ImapTransactionState.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapCommand.cs \
	Smdn.Net.Imap4.Protocol.Server/ImapMalformedCommandException.cs \
	Smdn.Net.Imap4.Server.Transaction.BuiltIn/DisconnectTransaction.cs \
	Smdn.Net.Imap4.Server.Transaction.BuiltIn/CapabilityTransaction.cs \
	Smdn.Net.Imap4.Server.Transaction.BuiltIn/NoOpTransaction.cs \
	Smdn.Net.Imap4.Server.Transaction.BuiltIn/LogoutTransaction.cs \
	Smdn.Net.Imap4.Server.Transaction.BuiltIn/InvalidCommandTransaction.cs \
	Smdn.Net.Imap4.Server/ImapMailbox.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Net.Imap4.Server \
	Smdn.Net.Imap4.Protocol.Server \
	Smdn.Net.Imap4.Server.Session \
	Smdn.Net.Imap4.Server.Transaction.BuiltIn \
	Smdn.Net.Imap4.Server.Transaction \
	smdn.net.imap4.server.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_NET_IMAP4_DLL = $(BUILD_DIR)/Smdn.Net.Imap4.dll
SMDN_SECURITY_SASL_DLL = $(BUILD_DIR)/Smdn.Security.Sasl.dll
SMDN_NET_IMAP4_SERVER_PC = $(BUILD_DIR)/smdn.net.imap4.server.pc

$(eval $(call emit-deploy-wrapper,SMDN_NET_IMAP4_SERVER_PC,smdn.net.imap4.server.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
