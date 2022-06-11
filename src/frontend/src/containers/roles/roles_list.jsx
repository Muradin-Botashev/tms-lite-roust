import React, { Component } from 'react';
import { connect } from 'react-redux';
import { withTranslation } from 'react-i18next';

import TableInfo from '../../components/TableInfo';

import { rolesColumns } from '../../constants/rolesColumns';
import {
    getRolesRequest,
    progressSelector,
    rolesListSelector,
    toggleRoleActiveRequest,
    totalCountSelector,
} from '../../ducks/roles';
import { Button, Icon } from 'semantic-ui-react';
import RoleCard from './role_card';
import { NEW_ROLE_LINK, ROLE_LINK } from '../../router/links';

const newModal = (t, load) => (
    <RoleCard title={t('create_role_title')} id={null} loadList={load}>
        <Button icon="add" />
    </RoleCard>
);

export class RolesList extends Component {
    handleToggleIsActive = (itemID, checked, load) => {
        const { toggleActive } = this.props;

        toggleActive({
            id: itemID,
            active: checked,
            callbackSuccess: () => {
                load(false, true);
            },
        });
    };

    getActions = (row, load, t) => {
        return [
            <RoleCard id={row.id} title={t('edit_role', { name: row.name })} loadList={load}>
                <Button size="mini" className="grid-action-btn">
                    <Icon name="edit" /> {t('edit_btn')}
                </Button>
            </RoleCard>,
        ];
    };

    getCard = ({ row, loadList, name }) => {
        const { t } = this.props;

        return <RoleCard title={t('edit_role', { name: row.name })} loadList={loadList} />;
    };

    render() {
        const { list, loadList, totalCount, loading, t } = this.props;

        return (
            <TableInfo
                headerRow={rolesColumns}
                name="roles"
                loading={loading}
                className="wider ui container container-margin-top-bottom"
                list={list}
                toggleIsActive={this.handleToggleIsActive}
                storageSortItem={`rolesSort`}
                storageFilterItem={`rolesFilters`}
                totalCount={totalCount}
                loadList={loadList}
                newLink={NEW_ROLE_LINK}
                cardLink={ROLE_LINK}
            />
        );
    }
}

const mapStateToProps = state => {
    return {
        list: rolesListSelector(state),
        totalCount: totalCountSelector(state),
        loading: progressSelector(state),
    };
};

const mapDispatchToProps = dispatch => {
    return {
        loadList: params => {
            dispatch(getRolesRequest(params));
        },
        toggleActive: params => {
            dispatch(toggleRoleActiveRequest(params));
        },
    };
};

export default withTranslation()(
    connect(
        mapStateToProps,
        mapDispatchToProps,
    )(RolesList),
);
