
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../build/bin/Debug/Smdn.Formats.Mime.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/Smdn.dll
BUILD_DIR = ../build/bin/Debug/

SMDN_DLL_SOURCE=../build/bin/Debug/Smdn.dll
SMDN_FORMATS_MIME_DLL_MDB_SOURCE=../build/bin/Debug/Smdn.Formats.Mime.dll.mdb
SMDN_FORMATS_MIME_DLL_MDB=$(BUILD_DIR)/Smdn.Formats.Mime.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize+
ASSEMBLY = ../build/bin/Release/Smdn.Formats.Mime.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/Smdn.dll
BUILD_DIR = ../build/bin/Release/

SMDN_DLL_SOURCE=../build/bin/Release/Smdn.dll
SMDN_FORMATS_MIME_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_DLL) \
	$(SMDN_FORMATS_MIME_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_FORMATS_MIME_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Smdn.Formats.Mime/MimeMessage.cs \
	AssemblyInfo.cs \
	Smdn.Formats.Mime/MimeType.cs \
	Smdn.Formats.Mime/MimeHeader.cs \
	Smdn.Formats.Mime/MimeHeaderEncoding.cs \
	Smdn.Formats.Mime/Charsets.cs \
	Smdn.Formats.Mime/MimeHeaderCollection.cs \
	Smdn.Formats.Mime/MimeHeaderFragment.cs \
	Smdn.Formats.Mime/IHeaderFieldBody.cs \
	Smdn.Formats.Mime/MimeHeaderParameterFragment.cs \
	Smdn.Formats.Mime/MimeMessageTransferEncoding.cs \
	Smdn.Formats.Mime/MimeFormat.cs \
	Smdn.Formats.Mime.Encode/Encoder.cs \
	Smdn.Formats.Mime.Encode/Formatter.cs \
	Smdn.Formats.Mime.Decode/Decoder.cs \
	Smdn.Formats.Mime.Decode/Parser.cs \
	Smdn.Formats.Mime/MimeMessageDisposition.cs \
	Smdn.Formats.Mime/NewLine.cs \
	Smdn.Formats.Mime.Encode/ContentEncodingStream.cs \
	Smdn.Formats.Mime.Encode/Base64ContentEncodingStream.cs \
	Smdn.Formats.Mime.Encode/QuotedPrintableContentEncodingStream.cs \
	Smdn.Formats.Mime.Decode/Base64ContentDecodingStream.cs \
	Smdn.Formats.Mime.Decode/QuotedPrintableContentDecodingStream.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Formats.Mime.Encode \
	Smdn.Formats.Mime.Decode \
	smdn.formats.mime.pc.in 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_FORMATS_MIME_PC = $(BUILD_DIR)/smdn.formats.mime.pc

$(eval $(call emit-deploy-wrapper,SMDN_FORMATS_MIME_PC,smdn.formats.mime.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
