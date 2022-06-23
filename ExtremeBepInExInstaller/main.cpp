#include <cstdio>
#include <iostream>
#include <filesystem>
#include <map>
#include <string>
#include <thread>
#include <windows.h>
#include <tlhelp32.h>
#include <atlstr.h>
#include <time.h>

using namespace std;
using namespace std::filesystem;

namespace
{
    
    const string defaultLang = "0";

    const map<string, map<string, string>> printString(
        {   
            {"11",
                {
                    {"dontClose"        , "!!!--作業が完了するまでこのウィンドウを閉じないで下さい--!!!"},
                    {"waitAmongUs"      , "Among Usの終了を待っています"},
                    {"removeBeplnEx"    , "古いバージョンのBeplnExを削除しています"},
                    {"installBeplnEx"   , "BeplnExをインストール中です"},
                    {"messageBoxSuccess", "BeplnExのインストールが完了しました。\nAmong Usを再起動して下さい"},
                    {"messageBoxFail"   , "BeplnExのインストールが失敗しました。\nExtreme Rolesを手動で導入して下さい"}
                }
            }
        });

    string GetTimeStmp()
    {
        time_t t = time(nullptr);
        struct tm localTime;
        localtime_s(&localTime, &t);

        std::stringstream s;
        s << "[";
        s << localTime.tm_year + 1900;
        s << ":";
        s << setw(2) << setfill('0') << localTime.tm_mon + 1;
        s << ":";
        s << setw(2) << setfill('0') << localTime.tm_mday;
        s << "T";
        s << setw(2) << setfill('0') << localTime.tm_hour;
        s << ":";
        s << setw(2) << setfill('0') << localTime.tm_min;
        s << ":";
        s << setw(2) << setfill('0') << localTime.tm_sec;
        s << "]:";

        return s.str();
    }

    bool InstallBepInEx(
        const string extractPath,
        const string gameRootPath)
    {
        try
        {
            copy(
                path(extractPath).concat("\\"),
                path(gameRootPath).concat("\\"),
                copy_options::update_existing | copy_options::recursive);
            return true;
        }
        catch (filesystem_error e)
        {
            cout << e.what() << endl;
            return false;
        }
    }

    bool IsProcessRunning(const wchar_t* processName)
    {
        bool exists = false;
        PROCESSENTRY32 entry;
        entry.dwSize = sizeof(PROCESSENTRY32);

        HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

        if (Process32First(snapshot, &entry))
        {
            while (Process32Next(snapshot, &entry))
            {
                if (!_wcsicmp(entry.szExeFile, processName))
                {
                    exists = true;
                }
            }
        }


        CloseHandle(snapshot);
        return exists;
    }

    void RemoveOldBeplnEx(const std::string gameRootPath)
    {
        path rootPath(gameRootPath);
        path bepInExPath = rootPath / "BepInEx";

        remove_all(bepInExPath / "core");
        remove_all(bepInExPath / "unhollowed");
        remove_all(bepInExPath / "unity-libs");

        remove(bepInExPath / "config" / "BepInEx.cfg");

        remove_all(rootPath / "mono");

        remove(rootPath / "changelog.txt");
        remove(rootPath / "doorstop_config.ini");
        remove(rootPath / "winhttp.dll");
    }
}


int main(int argc, char* argv[])
{
    cout << "--------------------  Extreme Roles - BepInEx Installer  --------------------" << endl;

    const string gameRootPath(argv[1]);
    const string extractPath(argv[2]);
    const string lang(argv[3]);
    const wstring processName(L"Among Us.exe");

    map<string, string> useStringData;

    if (printString.contains(lang))
    {
        useStringData = printString.at(lang);
    }
    else
    {
        useStringData = printString.at(defaultLang);
    }
    cout << useStringData.at("dontClose") << endl;
    cout << GetTimeStmp() << useStringData.at("waitAmongUs") << endl;

    while (IsProcessRunning(processName.c_str()))
    {
        this_thread::sleep_for(
            chrono::microseconds(20));
    }
    
    cout << GetTimeStmp() << useStringData.at("removeBeplnEx") << endl;
    RemoveOldBeplnEx(gameRootPath);
    
    cout << GetTimeStmp() << useStringData.at("installBeplnEx") << endl;
    bool result = InstallBepInEx(extractPath, gameRootPath);

    string showTextKey = result ? "messageBoxSuccess" : "messageBoxFail";

    CStringW cstringw(useStringData.at(showTextKey).c_str());

    MessageBoxW(NULL, cstringw,
        L"Extreme Roles - BepInEx Installer", MB_ICONINFORMATION | MB_OK);
}