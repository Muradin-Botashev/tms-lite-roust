import React, { Component } from 'react';
import { connect } from 'react-redux';
import { withTranslation } from 'react-i18next';
import { Button, Confirm, Dimmer, Form, Grid, Loader, Modal } from 'semantic-ui-react';
import {
    clearUserCard,
    createUserRequest,
    errorSelector,
    getUserCardRequest,
    progressSelector,
    userCardSelector,
} from '../../ducks/users';
import {
    getRolesRequest,
    progressSelector as rolesProgressSelector,
    rolesFromUserSelector,
} from '../../ducks/roles';
import { SELECT_TYPE, TEXT_TYPE } from '../../constants/columnTypes';
import FormField from '../../components/BaseComponents';

const initialState = {
    modalOpen: false,
    form: {},
    confirmation: { open: false },
    notChangeForm: true,
};

class UserCard extends Component {
    constructor(props) {
        super(props);

        this.state = {
            ...initialState,
            form: {
                login: null,
                userName: null,
                roleId: null,
                companyId: null,
                email: null,
                carrierId: null,
                isActive: true,
            },
        };
    }

    componentDidUpdate(prevProps, prevState) {
        if (prevProps.user !== this.props.user) {
            const { user = {} } = this.props;

            this.setState({
                form: {
                    login: user.login,
                    userName: user.userName,
                    roleId: user.roleId,
                    companyId: user.companyId,
                    carrierId: user.carrierId,
                    email: user.email,
                    password: user.password,
                    isActive: user.isActive,
                },
            });
        }
    }

    handleOpen = () => {
        const { getUser, id, getRoles } = this.props;

        id && getUser(id);
        getRoles({
            filter: {
                skip: 0,
                take: 20,
            },
        });
        this.setState({ modalOpen: true });
    };

    handleClose = () => {
        const { notChangeForm } = this.state;
        const { t } = this.props;

        if (notChangeForm) {
            this.confirmClose();
        } else {
            this.setState({
                confirmation: {
                    open: true,
                    content: t('confirm_close_dictionary'),
                    onCancel: () => {
                        this.setState({
                            confirmation: { open: false },
                        });
                    },
                    onConfirm: this.confirmClose,
                },
            });
        }
    };

    confirmClose = () => {
        const { loadList, clear } = this.props;
        this.setState({ ...initialState });
        clear();
        loadList(false, true);
    };

    handleChange = (event, { name, value }) => {
        this.setState(prevState => ({
            notChangeForm: false,
            form: {
                ...prevState.form,
                [name]: value,
            },
        }));
    };

    handleRoleChange = (event, { name, value }) => {
        this.handleChange(event, { name, value });
        this.handleChange(event, { name: 'carrierId', value: null });
    };

    mapProps = () => {
        const { form } = this.state;
        const { id } = this.props;

        let params = { ...form };

        if (id) {
            params = {
                ...params,
                id,
            };
        }

        return params;
    };

    handleCreate = () => {
        const { createUser } = this.props;

        createUser({ params: this.mapProps(), callbackFunc: this.confirmClose });
    };

    render() {
        const { modalOpen, form, confirmation } = this.state;
        const { login, userName, roleId, companyId, email, isActive, password, carrierId } = form;
        const { children, title, loading, t, error, user } = this.props;

        return (
            <Modal
                trigger={children}
                open={modalOpen}
                dimmer="blurring"
                closeIcon
                onOpen={this.handleOpen}
                onClose={this.handleClose}
            >
                <Modal.Header>{title}</Modal.Header>
                <Modal.Content>
                    <Dimmer active={loading} inverted className="table-loader">
                        <Loader size="huge">Loading</Loader>
                    </Dimmer>
                    <Form>
                        <Grid columns="equal">
                            <Grid.Row columns="equal">
                                <Grid.Column>
                                    <FormField
                                        type={TEXT_TYPE}
                                        name="email"
                                        value={email}
                                        isRequired
                                        error={error['email']}
                                        onChange={this.handleChange}
                                    />
                                    <FormField
                                        name="userName"
                                        value={userName}
                                        type={TEXT_TYPE}
                                        isRequired
                                        error={error['userName']}
                                        onChange={this.handleChange}
                                    />
                                    <FormField
                                        typeValue="password"
                                        name="password"
                                        isRequired={!user.id}
                                        value={password}
                                        type={TEXT_TYPE}
                                        error={error['password']}
                                        autoComplete="new-password"
                                        onChange={this.handleChange}
                                    />
                                    <FormField
                                        fluid
                                        search
                                        selection
                                        text="role"
                                        name="roleId"
                                        value={roleId}
                                        source="roles"
                                        isRequired
                                        error={error['roleId']}
                                        type={SELECT_TYPE}
                                        onChange={this.handleRoleChange}
                                    />
                                    <FormField
                                        fluid
                                        search
                                        selection
                                        name="companyId"
                                        value={companyId}
                                        source="companies"
                                        error={error['companyId']}
                                        type={SELECT_TYPE}
                                        onChange={this.handleChange}
                                    />
                                    <FormField
                                        fluid
                                        search
                                        selection
                                        name="carrierId"
                                        value={carrierId}
                                        source="transportCompanies"
                                        error={error['carrierId']}
                                        type={SELECT_TYPE}
                                        onChange={this.handleChange}
                                    />
                                    {/*{id ? (
                                            <Label pointing>
                                                Оставьте поле пустым, если не хотите менять пароль
                                            </Label>
                                        ) : null}*/}
                                    <Form.Field>
                                        <Form.Checkbox
                                            label={t('isActive')}
                                            name="isActive"
                                            checked={isActive}
                                            onChange={(e, { name, checked }) =>
                                                this.handleChange(e, { name, value: checked })
                                            }
                                        />
                                    </Form.Field>
                                </Grid.Column>
                            </Grid.Row>
                        </Grid>
                    </Form>
                </Modal.Content>
                <Modal.Actions>
                    <Button onClick={this.handleClose}>{t('CancelButton')}</Button>
                    <Button color="blue" onClick={this.handleCreate}>
                        {t('SaveButton')}
                    </Button>
                </Modal.Actions>
                <Confirm
                    dimmer="blurring"
                    open={confirmation.open}
                    onCancel={confirmation.onCancel}
                    cancelButton={t('cancelConfirm')}
                    onConfirm={confirmation.onConfirm}
                    content={confirmation.content}
                />
            </Modal>
        );
    }
}

const mapStateToProps = state => {
    return {
        user: userCardSelector(state),
        loading: progressSelector(state),
        roles: rolesFromUserSelector(state),
        rolesLoading: rolesProgressSelector(state),
        error: errorSelector(state),
    };
};

const mapDispatchToProps = dispatch => {
    return {
        getUser: params => {
            dispatch(getUserCardRequest(params));
        },
        getRoles: params => {
            dispatch(getRolesRequest(params));
        },
        createUser: params => {
            dispatch(createUserRequest(params));
        },
        clear: () => {
            dispatch(clearUserCard());
        },
    };
};

export default withTranslation()(
    connect(
        mapStateToProps,
        mapDispatchToProps,
    )(UserCard),
);
