
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize- -debug -r:Mono.Simd  "-define:NET_2_0;NET_3_5;NET_4_0;DEBUG"
ASSEMBLY = ../build/bin/Debug/netfx4.0/Smdn.Mathematics.SignalProcessing.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Debug/netfx4.0/Smdn.dll
BUILD_DIR = ../build/bin/Debug/netfx4.0/

SMDN_MATHEMATICS_SIGNALPROCESSING_DLL_MDB_SOURCE=../build/bin/Debug/netfx4.0/Smdn.Mathematics.SignalProcessing.dll.mdb
SMDN_MATHEMATICS_SIGNALPROCESSING_DLL_MDB=$(BUILD_DIR)/Smdn.Mathematics.SignalProcessing.dll.mdb
SMDN_DLL_SOURCE=../build/bin/Debug/netfx4.0/Smdn.dll
SMDN_DLL_MDB_SOURCE=../build/bin/Debug/netfx4.0/Smdn.dll.mdb
SMDN_DLL_MDB=$(BUILD_DIR)/Smdn.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:3 -optimize+ -r:Mono.Simd  "-define:NET_2_0;NET_3_5;NET_4_0"
ASSEMBLY = ../build/bin/Release/netfx4.0/Smdn.Mathematics.SignalProcessing.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../build/bin/Release/netfx4.0/Smdn.dll
BUILD_DIR = ../build/bin/Release/netfx4.0/

SMDN_MATHEMATICS_SIGNALPROCESSING_DLL_MDB=
SMDN_DLL_SOURCE=../build/bin/Release/netfx4.0/Smdn.dll
SMDN_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SMDN_MATHEMATICS_SIGNALPROCESSING_DLL_MDB) \
	$(SMDN_DLL) \
	$(SMDN_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SMDN_MATHEMATICS_SIGNALPROCESSING_NETFX4_0_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	Smdn.Mathematics.SignalProcessing.Transforms/Smpte370MDct.cs \
	Smdn.Mathematics.SignalProcessing.Transforms/Smpte370MFloatSimdDct.cs \
	Smdn.Mathematics.SignalProcessing.Transforms/Smpte370MIntegerSimdDct.cs \
	Smdn.Mathematics.SignalProcessing.Transforms/Smpte370MIntegerSisdDct.cs \
	Smdn.Mathematics.SignalProcessing.Transforms/ZigZag.cs \
	Smdn.Mathematics.SignalProcessing.Encoding/Smpte370MVlc.cs \
	Smdn.Mathematics.SignalProcessing.Encoding/Smpte370MVlcCodewords.cs \
	Smdn.Mathematics.SignalProcessing.Encoding/BitStream.cs \
	Smdn.Mathematics.SignalProcessing.Transforms/DctBlockInfo.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Smdn.Mathematics.SignalProcessing.Encoding \
	Smdn.Mathematics.SignalProcessing.Transforms \
	smdn.mathematics.signalprocessing-netfx4.0.pc.in 

REFERENCES =  \
	System \
	System.Core

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SMDN_DLL = $(BUILD_DIR)/Smdn.dll
SMDN_MATHEMATICS_SIGNALPROCESSING_NETFX4_0_PC = $(BUILD_DIR)/smdn.mathematics.signalprocessing-netfx4.0.pc

$(eval $(call emit-deploy-wrapper,SMDN_MATHEMATICS_SIGNALPROCESSING_NETFX4_0_PC,smdn.mathematics.signalprocessing-netfx4.0.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
