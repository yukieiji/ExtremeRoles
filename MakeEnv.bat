pip install -r requirements.txt
git submodule update --init --recursive
python makelanguagejson.py
if not exist ExtremeRoles\Resources\Asset mkdir ExtremeRoles\Resources\Asset
robocopy /mir UnityAsset\ExtremeRoles ExtremeRoles\Resources\Asset &amp; if errorlevel 8 (exit 1) else (exit 0)
if not exist ExtremeSkins\Resources\Asset mkdir ExtremeSkins\Resources\Asset
robocopy /mir UnityAsset\ExtremeSkins ExtremeSkins\Resources\Asset &amp; if errorlevel 8 (exit 1) else (exit 0)