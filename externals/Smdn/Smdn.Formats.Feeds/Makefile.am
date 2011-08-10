
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../build/bin/Debug/Smdn.Formats.Feeds.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.dll
BUILD_DIR = ../build/bin/Debug/

SMDN_DLL_SOURCE=../build/bin/Debug/Smdn.dll
SMDN_FORMATS_FEEDS_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Formats.Feeds.dll.mdb
SMDN_FORMATS_FEEDS_DLL_MDB=$(BUILD_DIR)/Smdn.Formats.Feeds.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = ../build/bin/Release/Smdn.Formats.Feeds.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.dll
BUILD_DIR = ../build/bin/Release/

SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_FORMATS_FEEDS_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_DLL) \
	$(SMDN_FORMATS_FEEDS_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_FORMATS_FEEDS_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Smdn.Formats.Feeds/EntryBase.cs \
	Smdn.Formats.Feeds/FeedBase.cs \
	Smdn.Formats.Feeds/FeedNamespaces.cs \
	Smdn.Formats.Feeds/FeedUri.cs \
	Smdn.Formats.Feeds/FeedVersion.cs \
	Smdn.Formats.Feeds/Formatter.cs \
	Smdn.Formats.Feeds/FeedPrefixes.cs \
	Smdn.Formats.Feeds/ParserCore.cs \
	Smdn.Formats.Feeds/FormatterCore.cs \
	Smdn.Formats.Feeds/Converter.cs \
	Smdn.Formats.Feeds/Conversion.cs \
	Smdn.Formats.Feeds/Parser.cs \
	Smdn.Formats.Feeds/IFeed.cs \
	Smdn.Formats.Feeds/IEntry.cs \
	Smdn.Formats.Feeds.Atom/Category.cs \
	Smdn.Formats.Feeds.Atom/Content.cs \
	Smdn.Formats.Feeds.Atom/Entry.cs \
	Smdn.Formats.Feeds.Atom/Feed.cs \
	Smdn.Formats.Feeds.Atom/FormatterImpl.cs \
	Smdn.Formats.Feeds.Atom/Generator.cs \
	Smdn.Formats.Feeds.Atom/Link.cs \
	Smdn.Formats.Feeds.Atom/ParserImpl.cs \
	Smdn.Formats.Feeds.Atom/Person.cs \
	Smdn.Formats.Feeds.Atom/Text.cs \
	Smdn.Formats.Feeds.Atom/TextType.cs \
	Smdn.Formats.Feeds.Modules/Annotation.cs \
	Smdn.Formats.Feeds.Modules/Content.cs \
	Smdn.Formats.Feeds.Modules/DublinCore.cs \
	Smdn.Formats.Feeds.Modules/Image.cs \
	Smdn.Formats.Feeds.Modules/ModuleBase.cs \
	Smdn.Formats.Feeds.Modules/Syndication.cs \
	Smdn.Formats.Feeds.Modules/Taxonomy.cs \
	Smdn.Formats.Feeds.Modules/Trackback.cs \
	Smdn.Formats.Feeds.RdfRss/Channel.cs \
	Smdn.Formats.Feeds.RdfRss/FormatterImpl.cs \
	Smdn.Formats.Feeds.RdfRss/Item.cs \
	Smdn.Formats.Feeds.RdfRss/ParserImpl.cs \
	Smdn.Formats.Feeds.Rss/Category.cs \
	Smdn.Formats.Feeds.Rss/Channel.cs \
	Smdn.Formats.Feeds.Rss/Cloud.cs \
	Smdn.Formats.Feeds.Rss/Enclosure.cs \
	Smdn.Formats.Feeds.Rss/FormatterImpl.cs \
	Smdn.Formats.Feeds.Rss/Guid.cs \
	Smdn.Formats.Feeds.Rss/Image.cs \
	Smdn.Formats.Feeds.Rss/Item.cs \
	Smdn.Formats.Feeds.Rss/ParserImpl.cs \
	Smdn.Formats.Feeds.Rss/Source.cs \
	Smdn.Formats.Feeds.Rss/TextInput.cs \
	AssemblyInfo.cs \
	Smdn.Formats.Feeds/VersionNotSupportedException.cs \
	Smdn.Formats.Feeds/FeedFormatException.cs \
	Smdn.Formats.Feeds/MandatoryValueMissingException.cs \
	Smdn.Formats.Feeds/EntryHashAlgorithm.cs \
	Smdn.Formats.Feeds/GenericFeed.cs \
	Smdn.Formats.Feeds/GenericEntry.cs \
	Smdn.Formats.Feeds/FeedMimeTypes.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	smdn.formats.feeds.pc.in 

REFERENCES =  \
	System \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_FORMATS_FEEDS_PC = $(BUILD_DIR)/smdn.formats.feeds.pc

$(eval $(call emit-deploy-wrapper,SMDN_FORMATS_FEEDS_PC,smdn.formats.feeds.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
