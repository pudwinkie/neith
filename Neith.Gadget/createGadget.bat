setlocal
path %PATH%;C:\Program Files\7-Zip
pushd neith_xiv
  7z a -tzip ..\neith_xiv.gadget *
  popd
