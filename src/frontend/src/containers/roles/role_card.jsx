import React, { Component } from 'react';
import { connect } from 'react-redux';
import { withTranslation } from 'react-i18next';
import { Button, Dimmer, Form, Input, Loader, Modal, Tab } from 'semantic-ui-react';
import {
    allActionsSelector,
    allPermissionsSelector,
    clearRolesCard,
    clearRolesInfo,
    createRoleRequest,
    errorSelector,
    getAllActionsRequest,
    getAllPermissionsRequest,
    getRoleCardRequest,
    progressSelector,
    roleCardSelector,
} from '../../ducks/roles';
import { sortFunc } from '../../utils/sort';
import FormField from '../../components/BaseComponents';
import { SELECT_TYPE, TEXT_TYPE } from '../../constants/columnTypes';

class RoleCard extends Component {
    constructor(props) {
        super(props);

        this.state = {
            modalOpen: false,
            form: {
                name: '',
                companyId: null,
                permissions: [],
                actions: [],
            },
        };
    }

    componentDidUpdate(prevProps) {
        if (prevProps.role !== this.props.role) {
            const { role = {} } = this.props;
            const { permissions = [] } = role;

            this.setState({
                form: {
                    ...role,
                    permissions: permissions.map(item => item.code),
                },
            });
        }
    }

    handleOpen = () => {
        const { getRole, id, getAllPermissions, getAllActions } = this.props;

        id && getRole(id);
        getAllPermissions && getAllPermissions();
        getAllActions && getAllActions();

        this.setState({
            modalOpen: true,
        });
    };

    handleClose = () => {
        const { loadList, clear } = this.props;

        this.setState({
            modalOpen: false,
        });

        this.setState({ form: {} });

        clear();

        loadList(false, true);
    };

    handleChange = (e, { name, value }) => {
        this.setState(prevState => ({
            form: {
                ...prevState.form,
                [name]: value,
            },
        }));
    };

    handlePermissions = (e, { value }) => {
        const { permissions } = this.state.form;

        const selectedPermissions = new Set(permissions);

        selectedPermissions[selectedPermissions.has(value) ? 'delete' : 'add'](value);

        if (value === 1 && !selectedPermissions.has(value)) {
            selectedPermissions.delete(2);
            selectedPermissions.delete(4);
            selectedPermissions.delete(5);
            selectedPermissions.delete(6);
        }

        if (value === 7 && !selectedPermissions.has(value)) {
            selectedPermissions.delete(10);
            selectedPermissions.delete(11);
            selectedPermissions.delete(12);
        }

        this.handleChange(null, { name: 'permissions', value: Array.from(selectedPermissions) });
    };

    handleActions = (e, { value }) => {
        const { actions } = this.state.form;

        const selectedActions = new Set(actions);

        selectedActions[selectedActions.has(value) ? 'delete' : 'add'](value);

        this.handleChange(null, { name: 'actions', value: Array.from(selectedActions) });
    };

    mapData = () => {
        const { form } = this.state;

        return {
            ...form,
            permissions: form.permissions.map(item => ({
                code: item,
            })),
        };
    };

    handleCreate = () => {
        const { createRole } = this.props;

        createRole({ params: this.mapData(), callbackFunc: this.handleClose });
    };

    render() {
        const { title, children, loading, t, allPermissions, allActions, error } = this.props;
        const { orderActions = [], shippingActions = [] } = allActions;
        const { modalOpen, form } = this.state;
        const { name, companyId, permissions = [], actions = [] } = form;

        return (
            <Modal
                trigger={children}
                open={modalOpen}
                dimmer="blurring"
                className="card-modal"
                closeIcon
                onOpen={this.handleOpen}
                onClose={this.handleClose}
            >
                <Modal.Header>{title}</Modal.Header>
                <Modal.Content scrolling>
                    <Dimmer active={loading} inverted className="table-loader">
                        <Loader size="huge">Loading</Loader>
                    </Dimmer>
                    <Form>
                        <FormField
                            name="name"
                            value={name}
                            type={TEXT_TYPE}
                            isRequired
                            error={error['name']}
                            onChange={this.handleChange}
                        />
                        <FormField
                            search
                            selection
                            name="companyId"
                            value={companyId}
                            source="companies"
                            error={error['companyId']}
                            type={SELECT_TYPE}
                            onChange={this.handleChange}
                        />
                        {/*<Form.Field>
                            <label>{t('name')}</label>
                            <Input value={name} name="name"  />
                        </Form.Field>*/}
                        {/*<Form.Field>
                            <label>{t('permissions')}</label>
                        </Form.Field>*/}
                        <Tab
                            panes={[
                                {
                                    menuItem: t('general'),
                                    render: () => (
                                        <Tab.Pane>
                                            {allPermissions.map(permission => (
                                                <Form.Field key={permission.code}>
                                                    <Form.Checkbox
                                                        label={t(permission.name)}
                                                        value={permission.code}
                                                        checked={
                                                            permissions &&
                                                            permissions.includes(permission.code)
                                                        }
                                                        disabled={
                                                            (permissions &&
                                                                ([2, 4, 5, 6].includes(
                                                                    permission.code,
                                                                ) &&
                                                                    !permissions.includes(1))) ||
                                                            ([10, 11, 12].includes(
                                                                permission.code,
                                                            ) &&
                                                                !permissions.includes(7))
                                                        }
                                                        onChange={this.handlePermissions}
                                                    />
                                                </Form.Field>
                                            ))}
                                        </Tab.Pane>
                                    ),
                                },
                                {
                                    menuItem: t('order_actions'),
                                    render: () => (
                                        <Tab.Pane>
                                            {sortFunc(orderActions, t).map(action => (
                                                <Form.Field key={action}>
                                                    <Form.Checkbox
                                                        label={t(action)}
                                                        value={action}
                                                        checked={
                                                            actions && actions.includes(action)
                                                        }
                                                        onChange={this.handleActions}
                                                    />
                                                </Form.Field>
                                            ))}
                                        </Tab.Pane>
                                    ),
                                },
                                {
                                    menuItem: t('shipping_actions'),
                                    render: () => (
                                        <Tab.Pane>
                                            {sortFunc(shippingActions, t).map(action => (
                                                <Form.Field key={action}>
                                                    <Form.Checkbox
                                                        label={t(action)}
                                                        value={action}
                                                        checked={
                                                            actions && actions.includes(action)
                                                        }
                                                        onChange={this.handleActions}
                                                    />
                                                </Form.Field>
                                            ))}
                                        </Tab.Pane>
                                    ),
                                },
                            ]}
                        />
                    </Form>
                </Modal.Content>
                <Modal.Actions>
                    <Button onClick={this.handleClose}>{t('CancelButton')}</Button>
                    <Button color="blue" onClick={this.handleCreate}>
                        {t('SaveButton')}
                    </Button>
                </Modal.Actions>
            </Modal>
        );
    }
}

const mapStateToProps = state => {
    return {
        role: roleCardSelector(state),
        loading: progressSelector(state),
        allPermissions: allPermissionsSelector(state),
        allActions: allActionsSelector(state),
        error: errorSelector(state),
    };
};

const mapDispatchToProps = dispatch => {
    return {
        getRole: params => {
            dispatch(getRoleCardRequest(params));
        },
        createRole: params => {
            dispatch(createRoleRequest(params));
        },
        clear: () => {
            dispatch(clearRolesCard());
        },
        getAllPermissions: () => {
            dispatch(getAllPermissionsRequest());
        },
        getAllActions: () => {
            dispatch(getAllActionsRequest());
        },
    };
};

export default withTranslation()(
    connect(
        mapStateToProps,
        mapDispatchToProps,
    )(RoleCard),
);
