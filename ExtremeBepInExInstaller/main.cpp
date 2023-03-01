#include <cstdio>
#include <iostream>
#include <filesystem>
#include <map>
#include <string>
#include <thread>
#include "windows.h"
#include "tlhelp32.h"
#include "time.h"

using namespace std;
using namespace std::filesystem;

namespace
{
    
    const string defaultLang = "0";

    const map<string, map<string, wstring>> printString(
        {   
            {"11",
                {
                    {"dontClose"        , L"!!!--作業が完了するまでこのウィンドウを閉じないで下さい--!!!"},
                    {"waitAmongUs"      , L"Among Usの終了を待っています"},
                    {"removeBeplnEx"    , L"古いバージョンのBeplnExを削除しています"},
                    {"installBeplnEx"   , L"BeplnExをインストール中です"},
                    {"messageBoxSuccess", L"BeplnExのインストールが完了しました。\nAmong Usを再起動して下さい"},
                    {"messageBoxFail"   , L"BeplnExのインストールが失敗しました。\nExtreme Rolesを手動で導入して下さい"}
                }
            }
        });

    wstring GetTimeStmp()
    {
        time_t t = time(nullptr);
        struct tm localTime;
        localtime_s(&localTime, &t);

        std::wstringstream s;
        s << L"[";
        s << localTime.tm_year + 1900;
        s << L":";
        s << setw(2) << setfill(L'0') << localTime.tm_mon + 1;
        s << L":";
        s << setw(2) << setfill(L'0') << localTime.tm_mday;
        s << L"T";
        s << setw(2) << setfill(L'0') << localTime.tm_hour;
        s << L":";
        s << setw(2) << setfill(L'0') << localTime.tm_min;
        s << L":";
        s << setw(2) << setfill(L'0') << localTime.tm_sec;
        s << L"]:";

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
        remove_all(rootPath / "dotnet");

        remove(rootPath / "changelog.txt");
        remove(rootPath / "doorstop_config.ini");
        remove(rootPath / "winhttp.dll");
    }
}


int main(int argc, char* argv[])
{
    wcout.imbue(std::locale(""));

    wcout << L"--------------------  Extreme Roles - BepInEx Installer  --------------------" << endl;

    const string gameRootPath(argv[1]);
    const string extractPath(argv[2]);
    const string lang(argv[3]);
    const wstring processName(L"Among Us.exe");

    map<string, wstring> useStringData;

    if (printString.contains(lang))
    {
        useStringData = printString.at(lang);
    }
    else
    {
        useStringData = printString.at(defaultLang);
    }
    wcout << useStringData.at("dontClose") << endl;
    wcout << GetTimeStmp() << useStringData.at("waitAmongUs") << endl;

    while (IsProcessRunning(processName.c_str()))
    {
        this_thread::sleep_for(
            chrono::microseconds(20));
    }
    
    wcout << GetTimeStmp() << useStringData.at("removeBeplnEx") << endl;
    RemoveOldBeplnEx(gameRootPath);
    
    wcout << GetTimeStmp() << useStringData.at("installBeplnEx") << endl;
    bool result = InstallBepInEx(extractPath, gameRootPath);

    string showTextKey = result ? "messageBoxSuccess" : "messageBoxFail";

    MessageBoxW(NULL, useStringData.at(showTextKey).c_str(),
        L"Extreme Roles - BepInEx Installer", MB_ICONINFORMATION | MB_OK);
}