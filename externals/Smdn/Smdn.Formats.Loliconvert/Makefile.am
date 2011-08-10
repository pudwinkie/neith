
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../build/bin/Debug/Smdn.Formats.Loliconvert.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../build/bin/Debug/

SMDN_FORMATS_LOLICONVERT_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Formats.Loliconvert.dll.mdb
SMDN_FORMATS_LOLICONVERT_DLL_MDB=$(BUILD_DIR)/Smdn.Formats.Loliconvert.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = ../build/bin/Release/Smdn.Formats.Loliconvert.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../build/bin/Release/

SMDN_FORMATS_LOLICONVERT_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_FORMATS_LOLICONVERT_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_FORMATS_LOLICONVERT_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Formats.Loliconvert/ToLoliTransform.cs \
	Smdn.Formats.Loliconvert/FromLoliTransform.cs \
	Smdn.Formats.Loliconvert/LoliOctets.cs \
	Smdn.Formats.Loliconvert/Runtime.cs \
	Smdn.Formats.Loliconvert/LoliStream.cs \
	Smdn.Formats.Loliconvert/LoliFile.cs \
	Smdn.Formats.Loliconvert/LoliconvertMode.cs \
	Smdn.Formats.Loliconvert/LolizationException.cs \
	Smdn.Formats.Loliconvert/LolisticException.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Formats.Loliconvert \
	smdn.formats.loliconvert.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_FORMATS_LOLICONVERT_PC = $(BUILD_DIR)/smdn.formats.loliconvert.pc

$(eval $(call emit-deploy-wrapper,SMDN_FORMATS_LOLICONVERT_PC,smdn.formats.loliconvert.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
