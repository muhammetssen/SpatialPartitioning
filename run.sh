
for n in {0..8}; 
do
    ./Build/SingleConnect.exe -batchmode -serverIndex $n -parcelCount 3 -objectCount 10 &

done
Build/SingleConnect.exe -parcelCount 3