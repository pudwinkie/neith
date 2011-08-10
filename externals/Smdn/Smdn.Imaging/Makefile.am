
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize- -debug "-define:NET_2_0;NET_3_5;NET_4_0;DEBUG"
ASSEMBLY = ../build/bin/Debug/netfx4.0/Smdn.Imaging.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/netfx4.0/Smdn.dll
BUILD_DIR = ../build/bin/Debug/netfx4.0/

SMDN_IMAGING_DLL_MDB_SOURCE=../build/bin/Debug/netfx4.0/Smdn.Imaging.dll.mdb
SMDN_IMAGING_DLL_MDB=$(BUILD_DIR)/Smdn.Imaging.dll.mdb
SMDN_DLL_SOURCE=../build/bin/Debug/netfx4.0/Smdn.dll
SMDN_DLL_MDB_SOURCE=../build/bin/Debug/netfx4.0/Smdn.dll.mdb
SMDN_DLL_MDB=$(BUILD_DIR)/Smdn.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize+ "-define:NET_2_0;NET_3_5;NET_4_0"
ASSEMBLY = ../build/bin/Release/netfx4.0/Smdn.Imaging.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/netfx4.0/Smdn.dll
BUILD_DIR = ../build/bin/Release/netfx4.0/

SMDN_IMAGING_DLL_MDB=
SMDN_DLL_SOURCE=../build/bin/Release/netfx4.0/Smdn.dll
SMDN_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_IMAGING_DLL_MDB) \
	$(SMDN_DLL) \
	$(SMDN_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_IMAGING_NETFX4_0_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Imaging/LockedBitmap.cs \
	Smdn.Imaging.Interop/wingdi.h.cs \
	Smdn.Imaging.Interop/windef.h.cs \
	Smdn.Imaging.Interop/gdi32.cs \
	Smdn.Imaging.Interop/user32.cs \
	Smdn.Imaging/AlignedRgbColor.cs \
	Smdn.Imaging/ColorDepth.cs \
	Smdn.Imaging/ColorModel.cs \
	Smdn.Imaging/AlignedHsvColor.cs \
	Smdn.Imaging/HsvColor.cs \
	Smdn.Imaging/GraphicsExtensions.cs \
	Smdn.Imaging/ImageFillStyle.cs \
	Smdn.Imaging/RectangleExtensions.cs \
	Smdn.Imaging/RectangleFExtensions.cs \
	Smdn.Imaging/PolarExtensions.cs \
	Smdn.Imaging/BitmapExtensions.cs \
	Smdn.Imaging/ImageCodecs.cs \
	Smdn.Imaging/IImageCodec.cs \
	Smdn.Imaging/IImageEncoder.cs \
	Smdn.Imaging/IImageDecoder.cs \
	Smdn.Imaging/IImageCodecs.cs \
	Smdn.Imaging/Codecs.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Imaging.Interop \
	smdn.imaging-netfx4.0.pc.in 

REFERENCES =  \
	System \
	System.Core \
	System.Drawing

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_IMAGING_NETFX4_0_PC = $(BUILD_DIR)/smdn.imaging-netfx4.0.pc

$(eval $(call emit-deploy-wrapper,SMDN_IMAGING_NETFX4_0_PC,smdn.imaging-netfx4.0.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
