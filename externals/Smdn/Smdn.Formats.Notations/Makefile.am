
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../build/bin/Debug/Smdn.Formats.Notations.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.Xml.dll \
	../build/bin/Debug/Smdn.Xml.Xhtml.dll \
	../build/bin/Debug/Smdn.dll
BUILD_DIR = ../build/bin/Debug/

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

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = ../build/bin/Release/Smdn.Formats.Notations.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.Xml.dll \
	../build/bin/Release/Smdn.Xml.Xhtml.dll \
	../build/bin/Release/Smdn.dll
BUILD_DIR = ../build/bin/Release/

SMDN_XML_DLL_SOURCE=../build/bin/Release/Smdn.Xml.dll
SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_DLL_MDB=
SMDN_XML_DLL_MDB=
SMDN_XML_XHTML_DLL_SOURCE=../build/bin/Release/Smdn.Xml.Xhtml.dll
SMDN_XML_XHTML_DLL_MDB=
SMDN_FORMATS_NOTATIONS_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_XML_DLL) \
	$(SMDN_DLL) \
	$(SMDN_DLL_MDB) \
	$(SMDN_XML_DLL_MDB) \
	$(SMDN_XML_XHTML_DLL) \
	$(SMDN_XML_XHTML_DLL_MDB) \
	$(SMDN_FORMATS_NOTATIONS_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_FORMATS_NOTATIONS_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Smdn.Formats.Notations/IFormatter.cs \
	Smdn.Formats.Notations/Parser.cs \
	Smdn.Formats.Notations/ProcessUnformattedCallback.cs \
	Smdn.Formats.Notations/ProcessUnparsedCallback.cs \
	Smdn.Formats.Notations/XhtmlFormatter.cs \
	AssemblyInfo.cs \
	Smdn.Formats.Notations/WikiLikeNotationParserBase.cs \
	Smdn.Formats.Notations.Dom/Container.cs \
	Smdn.Formats.Notations.Dom/Document.cs \
	Smdn.Formats.Notations.Dom/Node.cs \
	Smdn.Formats.Notations.Dom/NodeCollection.cs \
	Smdn.Formats.Notations.Dom/NodeCollection.ICollection.cs \
	Smdn.Formats.Notations.Dom/NodeCollection.IEnumerable.cs \
	Smdn.Formats.Notations.Dom/NodeCollection.IList.cs \
	Smdn.Formats.Notations.Dom/Anchor.cs \
	Smdn.Formats.Notations.Dom/Annotation.cs \
	Smdn.Formats.Notations.Dom/BlockCode.cs \
	Smdn.Formats.Notations.Dom/BlockQuotation.cs \
	Smdn.Formats.Notations.Dom/Comment.cs \
	Smdn.Formats.Notations.Dom/DefinitionList.cs \
	Smdn.Formats.Notations.Dom/DefinitionListItem.cs \
	Smdn.Formats.Notations.Dom/DeletedText.cs \
	Smdn.Formats.Notations.Dom/Emphasis.cs \
	Smdn.Formats.Notations.Dom/EmptyLine.cs \
	Smdn.Formats.Notations.Dom/EntityReference.cs \
	Smdn.Formats.Notations.Dom/ForcedLineBreak.cs \
	Smdn.Formats.Notations.Dom/Header.cs \
	Smdn.Formats.Notations.Dom/HorizontalRule.cs \
	Smdn.Formats.Notations.Dom/Image.cs \
	Smdn.Formats.Notations.Dom/Inline.cs \
	Smdn.Formats.Notations.Dom/InlineFrame.cs \
	Smdn.Formats.Notations.Dom/InsertedText.cs \
	Smdn.Formats.Notations.Dom/ListItem.cs \
	Smdn.Formats.Notations.Dom/OrderedList.cs \
	Smdn.Formats.Notations.Dom/Paragraph.cs \
	Smdn.Formats.Notations.Dom/Preformatted.cs \
	Smdn.Formats.Notations.Dom/StrongEmphasis.cs \
	Smdn.Formats.Notations.Dom/Table.cs \
	Smdn.Formats.Notations.Dom/TableCaption.cs \
	Smdn.Formats.Notations.Dom/TableColumn.cs \
	Smdn.Formats.Notations.Dom/TableFooterRow.cs \
	Smdn.Formats.Notations.Dom/TableHeaderColumn.cs \
	Smdn.Formats.Notations.Dom/TableHeaderRow.cs \
	Smdn.Formats.Notations.Dom/TableItem.cs \
	Smdn.Formats.Notations.Dom/TableRow.cs \
	Smdn.Formats.Notations.Dom/Text.cs \
	Smdn.Formats.Notations.Dom/UnorderedList.cs \
	Smdn.Formats.Notations/TextFormatter.cs \
	Smdn.Formats.Notations/Notation.cs \
	Smdn.Formats.Notations/ParserFormFeedEventArgs.cs \
	Smdn.Formats.Notations.Dom/Section.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Formats.Notations.Dom \
	smdn.formats.notations.pc.in 

REFERENCES =  \
	System \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_XML_DLL = $(BUILD_DIR)/Smdn.Xml.dll
SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_XML_XHTML_DLL = $(BUILD_DIR)/Smdn.Xml.Xhtml.dll
SMDN_FORMATS_NOTATIONS_PC = $(BUILD_DIR)/smdn.formats.notations.pc

$(eval $(call emit-deploy-wrapper,SMDN_FORMATS_NOTATIONS_PC,smdn.formats.notations.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
