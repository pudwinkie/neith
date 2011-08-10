#!/usr/bin/ruby

TEMPLATE_NAME = 'Smdn.Template'

class MakeLibDirTree
  def main
    if ARGV.length < 1 or ARGV[0] == ''
      STDERR.print "library name must be setted\n"
      exit
    end

    libname = ARGV[0]

    template_path = File.dirname($0)

    if template_path == ''
      template_path = "."
    end

    template_path += '/' + TEMPLATE_NAME

    print `mkdir -p #{libname}`
    copy_template(template_path, "./#{libname}", libname)

    print `svn add #{libname}/`
  end

  def copy_template(template_path, path, libname)
    Dir::glob("#{template_path}/*").each do |entry|
      next if entry == '.' or entry == '..'

      copyto = entry.gsub(template_path + '/', '').gsub(TEMPLATE_NAME, libname)

      if FileTest.file?(entry)
        copyfile = "#{path}/#{copyto}"

        `cp #{entry} #{copyfile}`

        # ファイルの場合はコピーしたファイル内を置き換える
        content = File.readlines(copyfile)

        csproj = (copyfile =~ /\.csproj$/)

        FileUtil.open(copyfile, 'w') do |file|
          content.each do |line|
            if csproj and line =~ /\{GUID\}/
              file << line.sub("{GUID}", "{#{`uuidgen -t`.chomp.upcase}}")
            else
              file << line.gsub(TEMPLATE_NAME, libname)
            end
          end
        end
      elsif FileTest.directory?(entry)
        copydir = "#{path}/#{copyto}"

        `mkdir #{copydir}`

        # ディレクトリの場合は、再帰的に処理する
        copy_template(entry, copydir, libname)
      end
    end
  end
end

class FileUtil
  def FileUtil.open(file, mode, &b)
    f = nil
    begin
      if File.exist?(file)
        f = File.open(file, mode)
      else
        f = File.open(file, 'w', 0644)
      end
      yield f
    ensure
      f.close if f
    end
  end
end

(MakeLibDirTree.new).main

