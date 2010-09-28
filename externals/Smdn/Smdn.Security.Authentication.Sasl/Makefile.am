
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../build/bin/Debug/Smdn.Security.Sasl.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.dll
BUILD_DIR = ../build/bin/Debug/

SMDN_DLL_SOURCE=../build/bin/Debug/Smdn.dll
SMDN_SECURITY_SASL_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Security.Sasl.dll.mdb
SMDN_SECURITY_SASL_DLL_MDB=$(BUILD_DIR)/Smdn.Security.Sasl.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize+
ASSEMBLY = ../build/bin/Release/Smdn.Security.Sasl.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.dll
BUILD_DIR = ../build/bin/Release/

SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_SECURITY_SASL_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_DLL) \
	$(SMDN_SECURITY_SASL_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_SECURITY_SASL_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Security.Sasl/SaslException.cs \
	Smdn.Security.Sasl.Client/SaslClientMechanism.cs \
	Smdn.Security.Sasl.Client/CRAMMD5Mechanism.cs \
	Smdn.Security.Sasl/SaslExchangeStatus.cs \
	Smdn.Security.Sasl.Client/DigestMD5Mechanism.cs \
	Smdn.Security.Sasl.Client/AnonymousMechanism.cs \
	Smdn.Security.Sasl.Client/LoginMechanism.cs \
	Smdn.Security.Sasl.Client/OTPMechanism.cs \
	Smdn.Security.Sasl.Client/PlainMechanism.cs \
	Smdn.Security.Sasl.Client/NTLMMechanism.cs \
	Smdn.Security.Sasl/SaslMechanismNameAttribute.cs \
	Smdn.Security.Sasl/SaslMechanismNotSupportedException.cs \
	Smdn.Security.Sspi/SspiTypes.cs \
	Smdn.Security.Sspi/Secur32.cs \
	Smdn.Security.Sspi/SspiException.cs \
	Smdn.Security.Sspi/SspiBase.cs \
	Smdn.Security.Sspi/SspiClient.cs \
	Smdn.Security.Sspi/SecBufferTypes.cs \
	Smdn.Security.Sasl.Client/NTLMMechanismImpl.cs \
	Smdn.Security.Sspi/SspiServer.cs \
	Smdn.Security.Sasl.Server/SaslServerMechanism.cs \
	Smdn.Security.Sasl.Server/NTLMMechanism.cs \
	Smdn.Security.Sasl.Server/NTLMMechanismImpl.cs \
	Smdn.Security.Sasl/Nonce.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Security.Sasl.Client \
	Smdn.Security.Sspi \
	Smdn.Security.Sasl.Server \
	smdn.security.sasl.pc.in 

REFERENCES =  \
	System \
	Mono.Security

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_SECURITY_SASL_PC = $(BUILD_DIR)/smdn.security.sasl.pc

$(eval $(call emit-deploy-wrapper,SMDN_SECURITY_SASL_PC,smdn.security.sasl.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
