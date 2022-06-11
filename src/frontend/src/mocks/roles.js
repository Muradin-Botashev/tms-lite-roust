export default {
    roles: [
        {
            id: 1,
            name: 'Administrator',
            users: 12,
            permissions: ['Роли', 'Пользователи'],
            is_active: true,
        },
        {
            id: 2,
            name: 'Planner',
            users: 12,
            permissions: [
                'Рейсы',
                'Заказы',
                'Требуют исправления',
                'Поставщики',
                'РЦ',
                'ПСГ',
                'Тарифы',
            ],
            is_active: true,
        },
    ],
};
