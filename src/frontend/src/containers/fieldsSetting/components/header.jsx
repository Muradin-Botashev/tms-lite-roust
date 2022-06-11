import React, {useMemo} from 'react';
import { Dropdown, Menu } from 'semantic-ui-react';
import _ from 'lodash';

const Header = ({ gridsList, activeItem, changeActiveItem, rolesList, role, changeRole, t, companiesList, company, changeCompany }) => {
    const rolesListOptions = useMemo(() => (
        [
            /*{ key: 'any_role', value: 'null', text: t('any_role') },*/
            ...rolesList.map(x => ({ key: x.name, value: x.value, text: x.name })),
        ]
    ), [rolesList]);

    const companyListOptions = useMemo(() => (
        [
            ...companiesList.map(x => ({ key: x.name, value: x.value, text: x.name })),
        ]
    ), [companiesList]);

    return (
        <Menu className="field-settings-menu">
            {gridsList && gridsList.length
                ? gridsList.map(item => (
                      <Menu.Item
                          key={item}
                          active={activeItem === item}
                          name={item}
                          onClick={changeActiveItem}
                      >
                          {t(item)}
                      </Menu.Item>
                  ))
                : null}
            <Menu.Item>
                <span>
                    <label>{`${t('role')}: `}</label>
                <Dropdown value={role} inline options={rolesListOptions} onChange={changeRole} />
                </span>
            </Menu.Item>
            <Menu.Item>
                <span>
                     <label>{`${t('ЮЛ')}: `}</label>
                    <Dropdown
                        value={company}
                        inline
                        options={companyListOptions}
                        onChange={changeCompany}
                    />
                </span>
                </Menu.Item>
        </Menu>
    );
};

export default React.memo(Header);
