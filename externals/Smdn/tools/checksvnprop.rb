#!/usr/bin/ruby

#usage: ./tools/checksvnprop.rb modify ./Smdn/AssemblyInfo.cs '*.cs'

base_file = nil
patterns = []
modify = false

ARGV.each do |arg|
  if arg == 'modify'
    modify = true
  elsif File.exist?(arg)
    base_file = arg
  else
    patterns << arg
  end
end

unless base_file
  print "base file must be specified"
  exit(-1)
end

if patterns.length <= 0
  print "pattern(s) must be specified"
  exit(-1)
end

props = ['svn:mime-type', 'svn:eol-style', 'svn:keywords']
base_props = {}

print "expected properties (#{base_file}):\n"
props.each do |prop|
  base_props[prop] = `svn propget #{prop} #{base_file}`.chomp

  print "  #{prop} = #{base_props[prop]}\n"
end

patterns.each do |pattern|
  print "checking files: #{pattern}\n"

  `find . -name '#{pattern}'`.each_line do |file|
    file = file.chomp

    props.each do |prop|
      file_prop = `svn propget #{prop} #{file}`.chomp
      if base_props[prop] != file_prop
        print "  #{file}: "
        if modify
          msg = `svn propset #{prop} '#{base_props[prop]}' '#{file}'`.chomp
          print "#{msg}\n"
        else
          print "#{prop} = '#{file_prop}' (expected = '#{base_props[prop]}')\n"
        end
      end
    end
  end
end

