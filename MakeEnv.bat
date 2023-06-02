pip install -r requirements.txt
git submodule update --init --recursive
python makelanguagejson.py
mkdir ExtremeRoles\Resources\Asset
robocopy /mir UnityAsset\ExtremeRoles ExtremeRoles\Resources\Asset
mkdir ExtremeSkins\Resources\Asset
robocopy /mir UnityAsset\ExtremeSkins ExtremeSkins\Resources\Asset