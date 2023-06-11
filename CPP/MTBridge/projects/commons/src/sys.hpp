#pragma once

#include <cstdlib>
#include <cstdint>
#include <cmath>
#include <type_traits>
#include <typeinfo>
#include <memory>
#include <memory_resource>
#include <utility>
#include <exception>
#include <algorithm>
#include <functional>
#include <array>
#include <set>
#include <map>
#include <queue>
#include <string>
#include <locale>
#include <codecvt>
#include <sstream>
#include <iostream>
#include <fstream>
#include <thread>
#include <atomic>
#include <mutex>
#include <condition_variable>
#include <future>
#include <chrono>
#include <regex>

#include <SDKDDKVer.h>
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN
#include <windows.h>

using byte_t = std::byte;
using int8_t = std::int8_t;
using int16_t = std::int16_t;
using int32_t = std::int32_t;
using int64_t = std::int64_t;
using uint8_t = std::uint8_t;
using uint16_t = std::uint16_t;
using uint32_t = std::uint32_t;
using uint64_t = std::uint64_t;

using string = std::string;
using wstring = std::wstring;
using namespace std::string_literals;

template<class T> using sptr = std::shared_ptr<T>;
template<class T, class Deleter = std::default_delete<T>> using uptr = std::unique_ptr<T, Deleter>;
