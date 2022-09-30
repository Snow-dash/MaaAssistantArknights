#pragma once

#include "AbstractResource.h"
#include "Utils/AsstTypes.h"

#include <memory>

namespace Map
{
    class TileCalc;
}

namespace asst
{
    class TilePack final : public SingletonHolder<TilePack>, public AbstractResource
    {
    public:
        enum class BuildableType
        {
            Invalid = -1,
            None = 0,
            Melee = 1,
            Ranged = 2,
            All = 3
        };
        enum class HeightType
        {
            Invalid = -1,
            Highland = 0,
            Floor = 1
        };
        enum class TileKey
        {
            Invalid = -1,
            Forbidden, // 不能放干员，敌人也不能走
            Wall,      // 可以放高台干员的位置
            Road,      // 可以放地面干员，敌人也可以走
            Home,      // 蓝门（可能还有其他的情况）
            EnemyHome, // 红门（可能还有其他的情况）
            Floor,     // 不能放干员，但敌人可以走
            Hole,      // 空降兵掉下去的地方（
            Telin,     // 传送门入口
            Telout,    // 传送门出口
            Volcano,   // 岩浆地块，可以放干员，敌人也可以走，但是会持续掉血
            Healing,   // 治疗地块，可以放干员，敌人也可以走，会给干员回血
            DeepSea, // 水，不能直接放干员（要借助泳圈道具），敌人可以走，但敌人会持续掉血
            Fence    // 敌人不会走，但可以放干员的地面位置
        };

        struct TileInfo
        {
            BuildableType buildable = BuildableType::Invalid;
            HeightType height = HeightType::Invalid;
            TileKey key = TileKey::Invalid;
            Point pos; // 像素坐标
            Point loc; // 格子位置
        };

    public:
        virtual ~TilePack() override;

        virtual bool load(const std::filesystem::path& path) override;

        std::unordered_map<Point, TileInfo> calc(const std::string& stage_code, bool side) const;

    private:
        std::shared_ptr<Map::TileCalc> m_tile_calculator = nullptr;
    };

    inline static auto& Tile = TilePack::get_instance();
}