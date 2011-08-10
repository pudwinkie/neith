
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../build/bin/Debug/Smdn.Formats.Notations.PukiWiki.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.Formats.Notations.dll \
	../build/bin/Debug/Smdn.Xml.dll \
	../build/bin/Debug/Smdn.Xml.Xhtml.dll \
	../build/bin/Debug/Smdn.dll
BUILD_DIR = ../build/bin/Debug/

SMDN_FORMATS_NOTATIONS_DLL_SOURCE=../build/bin/Debug/Smdn.Formats.Notations.dll
SMDN_XML_DLL_SOURCE=../build/bin/Debug/Smdn.Xml.dll
SMDN_DLL_SOURCE=../build/bin/Debug/Smdn.dll
SMDN_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.dll.mdb
SMDN_DLL_MDB=$(BUILD_DIR)/Smdn.dll.mdb
SMDN_XML_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Xml.dll.mdb
SMDN_XML_DLL_MDB=$(BUILD_DIR)/Smdn.Xml.dll.mdb
SMDN_XML_XHTML_DLL_SOURCE=../build/bin/Debug/Smdn.Xml.Xhtml.dll
SMDN_XML_XHTML_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Xml.Xhtml.dll.mdb
SMDN_XML_XHTML_DLL_MDB=$(BUILD_DIR)/Smdn.Xml.Xhtml.dll.mdb
SMDN_FORMATS_NOTATIONS_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Formats.Notations.dll.mdb
SMDN_FORMATS_NOTATIONS_DLL_MDB=$(BUILD_DIR)/Smdn.Formats.Notations.dll.mdb
SMDN_FORMATS_NOTATIONS_PUKIWIKI_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Formats.Notations.PukiWiki.dll.mdb
SMDN_FORMATS_NOTATIONS_PUKIWIKI_DLL_MDB=$(BUILD_DIR)/Smdn.Formats.Notations.PukiWiki.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = ../build/bin/Release/Smdn.Formats.Notations.PukiWiki.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.Formats.Notations.dll \
	../build/bin/Release/Smdn.Xml.dll \
	../build/bin/Release/Smdn.Xml.Xhtml.dll \
	../build/bin/Release/Smdn.dll
BUILD_DIR = ../build/bin/Release/

SMDN_FORMATS_NOTATIONS_DLL_SOURCE=../build/bin/Release/Smdn.Formats.Notations.dll
SMDN_XML_DLL_SOURCE=../build/bin/Release/Smdn.Xml.dll
SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_DLL_MDB=
SMDN_XML_DLL_MDB=
SMDN_XML_XHTML_DLL_SOURCE=../build/bin/Release/Smdn.Xml.Xhtml.dll
SMDN_XML_XHTML_DLL_MDB=
SMDN_FORMATS_NOTATIONS_DLL_MDB=
SMDN_FORMATS_NOTATIONS_PUKIWIKI_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_FORMATS_NOTATIONS_DLL) \
	$(SMDN_XML_DLL) \
	$(SMDN_DLL) \
	$(SMDN_DLL_MDB) \
	$(SMDN_XML_DLL_MDB) \
	$(SMDN_XML_XHTML_DLL) \
	$(SMDN_XML_XHTML_DLL_MDB) \
	$(SMDN_FORMATS_NOTATIONS_DLL_MDB) \
	$(SMDN_FORMATS_NOTATIONS_PUKIWIKI_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_FORMATS_NOTATIONS_PUKIWIKI_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Smdn.Formats.Notations.PukiWiki/Parser.cs \
	Smdn.Formats.Notations.PukiWiki.Dom/Alias.cs \
	Smdn.Formats.Notations.PukiWiki.Dom/AlignedParagraph.cs \
	Smdn.Formats.Notations.PukiWiki.Dom/BlockPlugin.cs \
	Smdn.Formats.Notations.PukiWiki.Dom/InlinePlugin.cs \
	Smdn.Formats.Notations.PukiWiki.Dom/Plugin.cs \
	AssemblyInfo.cs \
	Smdn.Formats.Notations.PukiWiki/XhtmlFormatter.cs \
	Smdn.Formats.Notations.PukiWiki.Dom/MultilineBlockPlugin.cs \
	Smdn.Formats.Notations.PukiWiki.Dom/HeaderAnchor.cs \
	Smdn.Formats.Notations.PukiWiki/TextFormatter.cs \
	Smdn.Formats.Notations.PukiWiki/Notation.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	smdn.formats.notations.pukiwiki.pc.in 

REFERENCES =  \
	System \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_FORMATS_NOTATIONS_DLL = $(BUILD_DIR)/Smdn.Formats.Notations.dll
SMDN_XML_DLL = $(BUILD_DIR)/Smdn.Xml.dll
SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_XML_XHTML_DLL = $(BUILD_DIR)/Smdn.Xml.Xhtml.dll
SMDN_FORMATS_NOTATIONS_PUKIWIKI_PC = $(BUILD_DIR)/smdn.formats.notations.pukiwiki.pc

$(eval $(call emit-deploy-wrapper,SMDN_FORMATS_NOTATIONS_PUKIWIKI_PC,smdn.formats.notations.pukiwiki.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
