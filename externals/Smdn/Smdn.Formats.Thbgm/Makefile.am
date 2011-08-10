
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../build/bin/Debug/Smdn.Formats.Thbgm.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.dll
BUILD_DIR = ../build/bin/Debug/

SMDN_DLL_SOURCE=../build/bin/Debug/Smdn.dll
SMDN_FORMATS_THBGM_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Formats.Thbgm.dll.mdb
SMDN_FORMATS_THBGM_DLL_MDB=$(BUILD_DIR)/Smdn.Formats.Thbgm.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = ../build/bin/Release/Smdn.Formats.Thbgm.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.dll
BUILD_DIR = ../build/bin/Release/

SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_FORMATS_THBGM_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_DLL) \
	$(SMDN_FORMATS_THBGM_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_FORMATS_THBGM_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Formats.Thbgm/ProductInfo.cs \
	Smdn.Formats.Thbgm/TrackInfo.cs \
	Smdn.Formats.Thbgm/StreamFormat.cs \
	Smdn.Formats.Thbgm/BgmStream.cs \
	Smdn.Formats.Thbgm/WaveStream.cs \
	Smdn.Formats.Thbgm/Repeat.cs 

DATA_FILES = 

RESOURCES = \
	titles_th06.txt \
	titles_th07.txt \
	titles_th08.txt \
	titles_th08tr.txt \
	titles_th09.txt \
	titles_th09tr.txt \
	titles_th10.txt \
	titles_th10tr.txt 

EXTRAS = \
	smdn.formats.thbgm.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_FORMATS_THBGM_PC = $(BUILD_DIR)/smdn.formats.thbgm.pc

$(eval $(call emit-deploy-wrapper,SMDN_FORMATS_THBGM_PC,smdn.formats.thbgm.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
