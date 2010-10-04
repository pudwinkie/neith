
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = ../build/bin/Debug/Smdn.Net.Imap4.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.dll
BUILD_DIR = ../build/bin/Debug/

SMDN_DLL_SOURCE=../build/bin/Debug/Smdn.dll
SMDN_NET_IMAP4_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Net.Imap4.dll.mdb
SMDN_NET_IMAP4_DLL_MDB=$(BUILD_DIR)/Smdn.Net.Imap4.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:TRACE"
ASSEMBLY = ../build/bin/Release/Smdn.Net.Imap4.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.dll
BUILD_DIR = ../build/bin/Release/

SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_NET_IMAP4_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_DLL) \
	$(SMDN_NET_IMAP4_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_NET_IMAP4_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Net.Imap4/ImapAddress.cs \
	Smdn.Net.Imap4/ImapEncoding.cs \
	Smdn.Net.Imap4/ImapEnvelope.cs \
	Smdn.Net.Imap4/ImapException.cs \
	Smdn.Net.Imap4/ImapFormatException.cs \
	Smdn.Net.Imap4/ImapNamespace.cs \
	Smdn.Net.Imap4/ImapNamespaceDesc.cs \
	Smdn.Net.Imap4/ImapString.cs \
	Smdn.Net.Imap4/ImapNilString.cs \
	Smdn.Net.Imap4/ImapStringEnum.cs \
	Smdn.Net.Imap4/ImapStringEnumList.cs \
	Smdn.Net.Imap4/ImapParenthesizedString.cs \
	Smdn.Net.Imap4/ImapStringList.cs \
	Smdn.Net.Imap4/ImapDateTimeFormat.cs \
	Smdn.Net.Imap4/ImapBodyStructureBase.cs \
	Smdn.Net.Imap4/ImapMultiPartBodyStructure.cs \
	Smdn.Net.Imap4/ImapSinglePartBodyStructure.cs \
	Smdn.Net.Imap4.Protocol/ImapDataFormat.cs \
	Smdn.Net.Imap4.Protocol/ImapConnectionException.cs \
	Smdn.Net.Imap4.Protocol/ImapDefaultPort.cs \
	Smdn.Net.Imap4/ImapUriForm.cs \
	Smdn.Net.Imap4/ImapCapability.cs \
	Smdn.Net.Imap4/ImapAuthenticationCapability.cs \
	Smdn.Net.Imap4/ImapAuthenticationMechanism.cs \
	Smdn.Net.Imap4/ImapCapabilityList.cs \
	Smdn.Net.Imap4/IImapExtension.cs \
	Smdn.Net.Imap4/IImapMultipleExtension.cs \
	Smdn.Net.Imap4/ImapCompressionMechanism.cs \
	Smdn.Net.Imap4/ImapThreadingAlgorithm.cs \
	Smdn.Net.Imap4/ImapQuotedString.cs \
	Smdn.Net.Imap4.Protocol/ImapData.cs \
	Smdn.Net.Imap4/IImapLiteralString.cs \
	Smdn.Net.Imap4/ImapLiteralStream.cs \
	Smdn.Net.Imap4/ImapLiteralString.cs \
	Smdn.Net.Imap4.Protocol/ImapBufferedStream.cs \
	Smdn.Net.Imap4.Protocol/ImapConnectionBase.cs \
	Smdn.Net.Imap4.Protocol/ImapUpgradeConnectionStreamCallback.cs \
	Smdn.Net.Imap4.Protocol/ImapOctets.cs \
	Smdn.Net.Imap4.Protocol/ImapReceiver.cs \
	Smdn.Net.Imap4.Protocol/ImapSender.cs \
	Smdn.Net.Imap4.Protocol/ImapMalformedDataException.cs \
	Smdn.Net.Imap4/ImapCombinableDataItem.cs \
	Smdn.Net.Imap4.Protocol/ImapDataConverter.cs \
	Smdn.Net.Imap4/ImapStatusAttributeList.cs \
	Smdn.Net.Imap4/ImapMessageFlag.cs \
	Smdn.Net.Imap4/ImapMessageFlagList.cs \
	Smdn.Net.Imap4/ImapMailboxFlag.cs \
	Smdn.Net.Imap4/ImapMailboxFlagList.cs \
	Smdn.Net.Imap4/ImapThreadList.cs \
	Smdn.Net.Imap4/ImapMailboxList.cs \
	Smdn.Net.Imap4/ImapSequenceSet.cs \
	Smdn.Net.Imap4/ImapMatchedSequenceSet.cs \
	Smdn.Net.Imap4/ImapUri.cs \
	Smdn.Net.Imap4/ImapUriBuilder.cs \
	Smdn.Net.Imap4/ImapMessage.cs \
	Smdn.Net.Imap4/ImapMailboxNameString.cs \
	Smdn.Net.Imap4/ImapSessionState.cs \
	Smdn.Net.Imap4.Protocol/ImapResponseCondition.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Net.Imap4 \
	Smdn.Net.Imap4.Protocol \
	smdn.net.imap4.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_NET_IMAP4_PC = $(BUILD_DIR)/smdn.net.imap4.pc

$(eval $(call emit-deploy-wrapper,SMDN_NET_IMAP4_PC,smdn.net.imap4.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
