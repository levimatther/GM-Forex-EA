#pragma once

#include "sys.hpp"

namespace commons
{
    struct dyntype
    {
        virtual ~dyntype() = default;
    };

    template<typename dst_tp, typename src_tp, std::enable_if_t<std::is_base_of<dyntype, dst_tp>::value, bool> = true>
    constexpr bool is_of_type(const src_tp& src) { return typeid(src) == typeid(dst_tp); }

    template<typename dst_tp, typename src_tp, std::enable_if_t<std::is_base_of<dyntype, dst_tp>::value, bool> = true>
    constexpr bool is_ptr_to_type(const src_tp* src) { return typeid(*src) == typeid(dst_tp); }

    template<typename memory_resource_tp>
    class object_pool
    {
    public:
        object_pool(memory_resource_tp& memory_resource) : m_memory_resource(memory_resource) { }

        object_pool(const object_pool&) = default;

        memory_resource_tp& resource() { return m_memory_resource; }

        template<typename type_tp, typename ...args_tps>
        type_tp * create(args_tps&&... args)
        {
            return new (m_memory_resource.allocate(sizeof(type_tp), alignof(type_tp))) type_tp(std::forward<args_tps>(args)...);
        }

    private:
        memory_resource_tp& m_memory_resource;
    };

    template<typename type_tp, typename mr_tp, typename ...args_tps>
    constexpr type_tp * allocate(mr_tp& mr, args_tps&&... args)
    {
        return new (mr.allocate(sizeof(type_tp), alignof(type_tp))) type_tp(std::forward<args_tps>(args)...);
    }

    template<typename enum_tp>
    struct named_enum
    {
        const enum_tp value;
        const string name;

        named_enum(enum_tp enum_p, string name_p) noexcept : value(enum_p), name(std::move(name_p)) { }
        operator int() noexcept { return value; }
        operator string() noexcept { return name; }
    };
}
