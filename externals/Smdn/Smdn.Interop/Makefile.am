
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize- -debug "-define:NET_2_0;NET_3_5;NET_4_0;DEBUG"
ASSEMBLY = ../build/bin/Debug/netfx4.0/Smdn.Interop.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/netfx4.0/Smdn.dll
BUILD_DIR = ../build/bin/Debug/netfx4.0/

SMDN_INTEROP_DLL_MDB_SOURCE=../build/bin/Debug/netfx4.0/Smdn.Interop.dll.mdb
SMDN_INTEROP_DLL_MDB=$(BUILD_DIR)/Smdn.Interop.dll.mdb
SMDN_DLL_SOURCE=../build/bin/Debug/netfx4.0/Smdn.dll
SMDN_DLL_MDB_SOURCE=../build/bin/Debug/netfx4.0/Smdn.dll.mdb
SMDN_DLL_MDB=$(BUILD_DIR)/Smdn.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize+ "-define:NET_2_0;NET_3_5;NET_4_0"
ASSEMBLY = ../build/bin/Release/netfx4.0/Smdn.Interop.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/netfx4.0/Smdn.dll
BUILD_DIR = ../build/bin/Release/netfx4.0/

SMDN_INTEROP_DLL_MDB=
SMDN_DLL_SOURCE=../build/bin/Release/netfx4.0/Smdn.dll
SMDN_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_INTEROP_DLL_MDB) \
	$(SMDN_DLL) \
	$(SMDN_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_INTEROP_NETFX4_0_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Interop/CoTaskMemoryBuffer.cs \
	Smdn.Interop/DynamicLibrary.cs \
	Smdn.Interop/DynamicLinkLibrary.cs \
	Smdn.Interop/GlobalMemoryBuffer.cs \
	Smdn.Interop/HeapMemoryBuffer.cs \
	Smdn.Interop/kernel32.cs \
	Smdn.Interop/libc.cs \
	Smdn.Interop/libdl.cs \
	Smdn.Interop/LocalMemoryBuffer.cs \
	Smdn.Interop/msvcrt.cs \
	Smdn.Interop/SharedLibrary.cs \
	Smdn.Interop/UnmanagedMemoryBuffer.cs \
	Smdn.Interop/FunctionNotFoundException.cs \
	Smdn.Interop/IntPtrExtensions.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Interop \
	smdn.interop-netfx4.0.pc.in 

REFERENCES =  \
	System \
	System.Core

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_INTEROP_NETFX4_0_PC = $(BUILD_DIR)/smdn.interop-netfx4.0.pc

$(eval $(call emit-deploy-wrapper,SMDN_INTEROP_NETFX4_0_PC,smdn.interop-netfx4.0.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
