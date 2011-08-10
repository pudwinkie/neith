
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = ../build/bin/Debug/Smdn.Net.Feeds.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.Formats.Feeds.dll
BUILD_DIR = ../build/bin/Debug/

SMDN_FORMATS_FEEDS_DLL_SOURCE=../build/bin/Debug/Smdn.Formats.Feeds.dll
SMDN_DLL_SOURCE=../build/bin/Debug/Smdn.dll
SMDN_NET_FEEDS_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Net.Feeds.dll.mdb
SMDN_NET_FEEDS_DLL_MDB=$(BUILD_DIR)/Smdn.Net.Feeds.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+ "-define:TRACE"
ASSEMBLY = ../build/bin/Release/Smdn.Net.Feeds.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.Formats.Feeds.dll
BUILD_DIR = ../build/bin/Release/

SMDN_FORMATS_FEEDS_DLL_SOURCE=../build/bin/Release/Smdn.Formats.Feeds.dll
SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_NET_FEEDS_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_FORMATS_FEEDS_DLL) \
	$(SMDN_DLL) \
	$(SMDN_NET_FEEDS_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_NET_FEEDS_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Net.Feeds/AggregationClientErrorException.cs \
	Smdn.Net.Feeds/AggregationContext.cs \
	Smdn.Net.Feeds/AggregationException.cs \
	Smdn.Net.Feeds/AggregationRedirectedException.cs \
	Smdn.Net.Feeds/AggregationServerErrorException.cs \
	Smdn.Net.Feeds/Aggregator.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	smdn.net.feeds.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_FORMATS_FEEDS_DLL = $(BUILD_DIR)/Smdn.Formats.Feeds.dll
SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_NET_FEEDS_PC = $(BUILD_DIR)/smdn.net.feeds.pc

$(eval $(call emit-deploy-wrapper,SMDN_NET_FEEDS_PC,smdn.net.feeds.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
