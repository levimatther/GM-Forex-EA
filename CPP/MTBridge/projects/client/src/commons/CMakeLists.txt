﻿list(APPEND CMAKE_PREFIX_PATH "${CMAKE_HOME_DIRECTORY}/vcpkg/packages/zeromq_x64-windows/share/zeromq")
find_package(ZeroMQ CONFIG REQUIRED)

include_directories(${ZeroMQ_INCLUDE_DIR})
list(APPEND LIB_HDR "src/condition.hpp" "src/etc.hpp" "src/result.hpp" "src/worker.hpp")
list(APPEND LIB_HDR "src/messaging.hpp")
list(APPEND LIB_SRC "src/messaging.cpp")
add_library(commons STATIC ${LIB_HDR} ${LIB_SRC})
target_precompile_headers(commons PUBLIC "src/sys.hpp")
target_link_libraries(commons PRIVATE libzmq)
