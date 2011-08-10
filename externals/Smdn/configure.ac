dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([libs], [0.20])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

AC_PATH_PROG(GMCS, gmcs, no)
if test "x$GMCS" = "xno"; then
        AC_MSG_ERROR([gmcs Not found])
fi


AC_ARG_ENABLE(default,
	AC_HELP_STRING([--enable-default],
		[Use 'DEFAULT' Configuration [default=NO]]),
		enable_default=yes, enable_default=no)
AM_CONDITIONAL(ENABLE_DEFAULT, test x$enable_default = xyes)
if test "x$enable_default" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=NO]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=YES]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)
if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_RELEASE, true)
	enable_release=yes
fi


dnl package checks, common for all configs

dnl package checks, per config


AC_CONFIG_FILES([
Smdn/smdn.pc
Smdn/Makefile
Smdn.Formats.Csv/smdn.formats.csv.pc
Smdn.Formats.Csv/Makefile
Smdn.Formats.Feeds/smdn.formats.feeds.pc
Smdn.Formats.Feeds/Makefile
Smdn.Formats.Ini/smdn.formats.ini.pc
Smdn.Formats.Ini/Makefile
Smdn.Formats.Mime/smdn.formats.mime.pc
Smdn.Formats.Mime/Makefile
Smdn.Formats.Notations/smdn.formats.notations.pc
Smdn.Formats.Notations/Makefile
Smdn.Formats.Notations.Hatena/smdn.formats.notations.hatena.pc
Smdn.Formats.Notations.Hatena/Makefile
Smdn.Formats.Notations.PukiWiki/smdn.formats.notations.pukiwiki.pc
Smdn.Formats.Notations.PukiWiki/Makefile
Smdn.Formats.Opml/smdn.formats.opml.pc
Smdn.Formats.Opml/Makefile
Smdn.Formats.Riff/smdn.formats.riff.pc
Smdn.Formats.Riff/Makefile
Smdn.IO.Maildir/smdn.io.maildir.pc
Smdn.IO.Maildir/Makefile
Smdn.Imaging/smdn.imaging.pc
Smdn.Imaging/Makefile
Smdn.Net.Feeds/smdn.net.feeds.pc
Smdn.Net.Feeds/Makefile
Smdn.Net.Imap4/smdn.net.imap4.pc
Smdn.Net.Imap4/Makefile
Smdn.Security.Sasl/smdn.security.sasl.pc
Smdn.Security.Sasl/Makefile
Smdn.Net.Pop3/smdn.net.pop3.pc
Smdn.Net.Pop3/Makefile
Smdn.Formats.Thbgm/smdn.formats.thbgm.pc
Smdn.Formats.Thbgm/Makefile
Smdn.Interop.WinMM/smdn.interop.winmm.pc
Smdn.Interop.WinMM/Makefile
Smdn.Interop.Alsa/smdn.interop.alsa.pc
Smdn.Interop.Alsa/Makefile
Smdn.Mathematics.SignalProcessing/smdn.mathematics.signalprocessing.pc
Smdn.Mathematics.SignalProcessing/Makefile
Smdn.Formats.Earthsoft.PV4/smdn.formats.earthsoft.pv4.pc
Smdn.Formats.Earthsoft.PV4/Makefile
Smdn.Formats.IsoBaseMediaFile/smdn.formats.isobasemediafile.pc
Smdn.Formats.IsoBaseMediaFile/Makefile
Smdn.Formats.MP4/smdn.formats.mp4.pc
Smdn.Formats.MP4/Makefile
Smdn.Formats.MP4.SmdnExtensions/smdn.formats.mp4.smdnextensions.pc
Smdn.Formats.MP4.SmdnExtensions/Makefile
Smdn.Formats.Loliconvert/smdn.formats.loliconvert.pc
Smdn.Formats.Loliconvert/Makefile
Smdn.Media.Earthsoft.PV4.Codec/smdn.media.earthsoft.pv4.codec.pc
Smdn.Media.Earthsoft.PV4.Codec/Makefile
Smdn.Media.Earthsoft.PV4.Multiplex/smdn.media.earthsoft.pv4.multiplex.pc
Smdn.Media.Earthsoft.PV4.Multiplex/Makefile
Smdn.Net.Imap4.Client/smdn.net.imap4.client.pc
Smdn.Net.Imap4.Client/Makefile
Smdn.Net.Imap4.Server/smdn.net.imap4.server.pc
Smdn.Net.Imap4.Server/Makefile
Makefile

])

AC_OUTPUT
