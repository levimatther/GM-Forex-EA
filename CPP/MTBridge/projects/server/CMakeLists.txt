cmake_minimum_required(VERSION 3.19)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /EHsc /wd4651")

project("MTBridgeServer")

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED TRUE)

add_subdirectory("src/commons")
add_subdirectory("src/shared")

include_directories("${CMAKE_HOME_DIRECTORY}/src/commons/src" "${CMAKE_HOME_DIRECTORY}/src/shared/src")
list(APPEND LIB_HDR "src/mt5_bridge.hpp")
list(APPEND LIB_SRC "src/mt5_bridge.cpp" "src/dll_main.cpp")
add_library(mt5bridge_s SHARED ${LIB_HDR} ${LIB_SRC})
target_precompile_headers(mt5bridge_s REUSE_FROM commons)
target_link_libraries(mt5bridge_s PRIVATE commons)
